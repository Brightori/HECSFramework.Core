using HECSFramework.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HECSFramework.Core.Generator
{
    public partial class CodeGenerator
    {
#pragma warning disable
        private readonly string DefaultPath = "/Scripts/HECSGenerated/";
        private readonly string ComponentsPath = "/Scripts/Components/";
        private readonly string DefaultNameSpace = "HECSFramework.Core";
        private readonly string TypesMap = "TypesMap.cs";
        private readonly string ComponentID = "ComponentID.cs";
        private readonly string ComponentsMask = "ComponentsMask.cs";
        private readonly string ComponentsContext = "ComponentContext.cs";
        private readonly string PartialWorld = "WorldPart.cs";
        private readonly string EntityGenericExtentions = "EntityGenericExtentions.cs";
#pragma warning enable

        private const string AggressiveInline = "[MethodImpl(MethodImplOptions.AggressiveInlining)]";

        private List<Type> componentTypes;
        private List<Type> componentTypesByClass;

        private List<Type> localSystemBind;
        private List<Type> globalSystemBind;

        private List<Type> systems;
        public static IEnumerable<Type> Assembly;


        public void StartGeneration()
        {
            //GatherAssembly();
            //GenerateMaskProvider();
            //GenerateTypesMap();
            //GenerateComponentContext();
        }

        #region SystemBind
        public string GetSystemBinds()
        {
            var tree = new TreeSyntaxNode();
            var bindSystemFunc = new TreeSyntaxNode();
            tree.Add(new UsingSyntax("Commands", 1));
            tree.Add(new NameSpaceSyntax("HECSFramework.Core"));
            tree.Add(new LeftScopeSyntax());
            tree.Add(new TabSimpleSyntax(1, "public partial class RegisterService"));
            tree.Add(new LeftScopeSyntax(1));
            tree.Add(bindSystemFunc);
            tree.Add(new RightScopeSyntax(1));
            tree.Add(new RightScopeSyntax());

            bindSystemFunc.Add(new TabSimpleSyntax(2, "partial void BindSystem(ISystem system)"));
            bindSystemFunc.Add(new LeftScopeSyntax(2));
            bindSystemFunc.Add(GetGlobalBindings());
            bindSystemFunc.Add(GetLocalBindings());
            bindSystemFunc.Add(new RightScopeSyntax(2));

            return tree.ToString();
        }

        private ISyntax GetGlobalBindings()
        {
            var tree = new TreeSyntaxNode();

            for (int i = 0; i < globalSystemBind.Count; i++)
            {
                Type t = globalSystemBind[i];

                if (i != 0)
                    tree.Add(new ParagraphSyntax());

                tree.Add(new TabSimpleSyntax(3, $"if (system is IReactGlobalCommand<{t.Name}> {t.Name}GlobalCommandsReact)"));
                tree.Add(new TabSimpleSyntax(4, $"system.Owner.World.AddGlobalReactCommand<{t.Name}>(system, {t.Name}GlobalCommandsReact.CommandGlobalReact);"));
            }

            return tree;
        }

        private ISyntax GetLocalBindings()
        {
            var tree = new TreeSyntaxNode();


            foreach (var t in localSystemBind)
            {
                tree.Add(new ParagraphSyntax());
                tree.Add(new TabSimpleSyntax(3, $"if (system is IReactCommand<{t.Name}> {t.Name}CommandsReact)"));
                tree.Add(new TabSimpleSyntax(4, $"system.Owner.EntityCommandService.AddListener<{t.Name}>(system, {t.Name}CommandsReact.CommandReact);"));
            }

            return tree;
        }
        #endregion

        #region GenerateTypesMap
        public string GenerateTypesMap()
        {
            var tree = new TreeSyntaxNode();
            var componentsSegment = new TreeSyntaxNode();

            tree.Add(new UsingSyntax("System.Collections.Generic"));
            tree.Add(new UsingSyntax("Components"));
            tree.Add(new UsingSyntax("System", 1));
            tree.Add(new UsingSyntax("Systems", 1));

            tree.Add(new NameSpaceSyntax(DefaultNameSpace));
            tree.Add(new LeftScopeSyntax());
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(1), new SimpleSyntax($"public partial class {typeof(TypesProvider).Name}"), new ParagraphSyntax()));
            tree.Add(new LeftScopeSyntax(1));
            tree.Add(new TabSimpleSyntax(2, "public TypesProvider()"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(new TabSimpleSyntax(3, $"Count = {componentTypes.Count + 1};"));
            tree.Add(new TabSimpleSyntax(3, $"MapIndexes = GetMapIndexes();"));
            tree.Add(new TabSimpleSyntax(3, $"TypeToComponentIndex = GetTypeToComponentIndexes();"));
            tree.Add(new TabSimpleSyntax(3, $"HashToType = GetHashToTypeDictionary();"));
            tree.Add(new TabSimpleSyntax(3, $"TypeToHash = GetTypeToHash();"));
            tree.Add(new TabSimpleSyntax(3, $"HECSFactory = new HECSFactory();"));
            tree.Add(new RightScopeSyntax(2));

            tree.Add(GetTypeToComponentIndexes());
            tree.Add(GetTypeToHash());
            tree.Add(GetHashToTypeDictionary());

            //dictionary
            tree.Add(new ParagraphSyntax());
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(2), new SimpleSyntax("private Dictionary<int, ComponentMaskAndIndex> GetMapIndexes()")));
            tree.Add(new ParagraphSyntax());
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(new TabSimpleSyntax(3, "return new Dictionary<int, ComponentMaskAndIndex>"));
            tree.Add(new LeftScopeSyntax(3));
            tree.Add(componentsSegment);
            tree.Add(new RightScopeSyntax(3, true));
            tree.Add(new RightScopeSyntax(2));
            tree.Add(new RightScopeSyntax(1));
            tree.Add(GetHECSComponentFactory());
            tree.Add(new RightScopeSyntax());

            //default stroke in dictionary
            componentsSegment.Add(new CompositeSyntax(new TabSpaceSyntax(4),
                new SimpleSyntax(@"{ -1, new ComponentMaskAndIndex {  ComponentName = ""DefaultEmpty"", ComponentsMask = HECSMask.Empty }},")));
            componentsSegment.Add(new ParagraphSyntax());

            //here we know how much mask field we have
            var m = ComponentsCount();

            for (int i = 0; i < componentTypesByClass.Count; i++)
            {
                Type c = componentTypesByClass[i];
                var index = i;
                componentsSegment.Add(GetComponentForTypeMap(index, m, c));
            }

            return tree.ToString();
        }

        private ISyntax GetHECSComponentFactory()
        {
            var tree = new TreeSyntaxNode();
            var constructor = new TreeSyntaxNode();
            var getComponentFunc = new TreeSyntaxNode();
            
            tree.Add(new ParagraphSyntax());
            tree.Add(new TabSimpleSyntax(1, "public partial class HECSFactory"));
            tree.Add(new LeftScopeSyntax(1));
            tree.Add(constructor);
            tree.Add(getComponentFunc);
            tree.Add(new RightScopeSyntax(1));

            constructor.Add(new TabSimpleSyntax(2, "public HECSFactory()"));
            constructor.Add(new LeftScopeSyntax(2));
            constructor.Add(new TabSimpleSyntax(3, "getComponentFromFactoryByHash = GetComponentFromFactoryFunc;"));
            constructor.Add(new TabSimpleSyntax(3, "getSystemFromFactoryByHash = GetSystemFromFactoryFunc;"));
            constructor.Add(new RightScopeSyntax(2));

            getComponentFunc.Add(new ParagraphSyntax());
            getComponentFunc.Add(new TabSimpleSyntax(2, "private IComponent GetComponentFromFactoryFunc(int hashCodeType)"));
            getComponentFunc.Add(new LeftScopeSyntax(2));
            getComponentFunc.Add(new TabSimpleSyntax(3, "switch (hashCodeType)"));
            getComponentFunc.Add(new LeftScopeSyntax(3));
            getComponentFunc.Add(GetComponentsByHashCode());
            getComponentFunc.Add(new RightScopeSyntax(3));
            getComponentFunc.Add(new ParagraphSyntax());
            getComponentFunc.Add(new TabSimpleSyntax(3, "return default;"));
            getComponentFunc.Add(new RightScopeSyntax(2));        
            
            getComponentFunc.Add(new ParagraphSyntax());
            getComponentFunc.Add(new TabSimpleSyntax(2, "private ISystem GetSystemFromFactoryFunc(int hashCodeType)"));
            getComponentFunc.Add(new LeftScopeSyntax(2));
            getComponentFunc.Add(new TabSimpleSyntax(3, "switch (hashCodeType)"));
            getComponentFunc.Add(new LeftScopeSyntax(3));
            getComponentFunc.Add(GetSystemsByHashCode());
            getComponentFunc.Add(new RightScopeSyntax(3));
            getComponentFunc.Add(new ParagraphSyntax());
            getComponentFunc.Add(new TabSimpleSyntax(3, "return default;"));
            getComponentFunc.Add(new RightScopeSyntax(2));
            
            return tree;
        }

        private ISyntax GetComponentsByHashCode()
        {
            var tree = new TreeSyntaxNode();

            for (int i = 0; i < componentTypes.Count; i++)
            {
                if (i > 0)
                    tree.Add(new ParagraphSyntax());

                var component = componentTypes[i];

                tree.Add(new TabSimpleSyntax(4, $"case {IndexGenerator.GetIndexForType(component)}:"));
                tree.Add(new TabSimpleSyntax(5, $"return new {component.Name}();"));
            }

            return tree;
        }
        
        private ISyntax GetSystemsByHashCode()
        {
            var tree = new TreeSyntaxNode();

            for (int i = 0; i < systems.Count; i++)
            {
                if (i > 0)
                    tree.Add(new ParagraphSyntax());

                var component = systems[i];

                tree.Add(new TabSimpleSyntax(4, $"case {IndexGenerator.GetIndexForType(component)}:"));
                tree.Add(new TabSimpleSyntax(5, $"return new {component.Name}();"));
            }

            return tree;
        }

        private ISyntax GetHashToTypeDictionary()
        {
            var tree = new TreeSyntaxNode();

            var dicBody = new TreeSyntaxNode();

            tree.Add(new ParagraphSyntax());
            tree.Add(new TabSimpleSyntax(2, "private Dictionary<int, Type> GetHashToTypeDictionary()"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(new TabSimpleSyntax(3, "return new Dictionary<int, Type>()"));
            tree.Add(new LeftScopeSyntax(3));
            tree.Add(dicBody);
            tree.Add(new RightScopeSyntax(3, true));
            tree.Add(new RightScopeSyntax(2));

            for (int i = 0; i < componentTypes.Count; i++)
            {
                var hash = IndexGenerator.GetIndexForType(componentTypes[i]);
                dicBody.Add(new TabSimpleSyntax(4, $"{{ {hash}, typeof({componentTypes[i].Name})}},"));
            }

            return tree;
        }

        private ISyntax GetTypeToComponentIndexes()
        {
            var tree = new TreeSyntaxNode();
            var dicBody = new TreeSyntaxNode();

            tree.Add(new ParagraphSyntax());
            tree.Add(new TabSimpleSyntax(2, "private Dictionary<Type, int> GetTypeToComponentIndexes()"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(new TabSimpleSyntax(3, "return new Dictionary<Type, int>()"));
            tree.Add(new LeftScopeSyntax(3));
            tree.Add(dicBody);
            tree.Add(new RightScopeSyntax(3, true));
            tree.Add(new RightScopeSyntax(2));

            for (int i = 0; i < componentTypes.Count; i++)
            {
                var interfaces = componentTypes[i].GetInterfaces();

                foreach (var @interface in interfaces)
                {
                    if (@interface.Name.Contains(componentTypes[i].Name))
                        dicBody.Add(new TabSimpleSyntax(4, $"{{ typeof({@interface.Name}), {i} }},"));
                }

                dicBody.Add(new TabSimpleSyntax(4, $"{{ typeof({componentTypes[i].Name}), {i} }},"));
            }

            return tree;
        }
        
        private ISyntax GetTypeToHash()
        {
            var tree = new TreeSyntaxNode();
            var dicBody = new TreeSyntaxNode();

            tree.Add(new ParagraphSyntax());
            tree.Add(new TabSimpleSyntax(2, "private Dictionary<Type, int> GetTypeToHash()"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(new TabSimpleSyntax(3, "return new Dictionary<Type, int>()"));
            tree.Add(new LeftScopeSyntax(3));
            tree.Add(dicBody);
            tree.Add(new RightScopeSyntax(3, true));
            tree.Add(new RightScopeSyntax(2));

            for (int i = 0; i < componentTypes.Count; i++)
            {
                var interfaces = componentTypes[i].GetInterfaces();
                var hash = IndexGenerator.GetIndexForType(componentTypes[i]);

                foreach (var @interface in interfaces)
                {
                    if (@interface.Name.Contains(componentTypes[i].Name))
                        dicBody.Add(new TabSimpleSyntax(4, $"{{ typeof({@interface.Name}), {hash} }},"));
                }

                dicBody.Add(new TabSimpleSyntax(4, $"{{ typeof({componentTypes[i].Name}), {hash} }},"));
            }

            return tree;
        }

        private ISyntax GetComponentForTypeMap(int index, int fieldCount, Type c)
        {
            var composite = new TreeSyntaxNode();
            var MaskPart = new TreeSyntaxNode();
            var maskBody = new TreeSyntaxNode();

            composite.Add(new ParagraphSyntax());
            composite.Add(new TabSpaceSyntax(3));
            composite.Add(new SimpleSyntax(CParse.LeftScope));
            composite.Add(new CompositeSyntax(new SimpleSyntax(CParse.Space+IndexGenerator.GetIndexForType(c).ToString() + CParse.Comma)));
            composite.Add(new SimpleSyntax($" new ComponentMaskAndIndex {{ComponentName = {CParse.Quote}{ c.Name }{(CParse.Quote)}, ComponentsMask = new {typeof(HECSMask).Name}"));
            composite.Add(new ParagraphSyntax());
            composite.Add(MaskPart);
            composite.Add(new CompositeSyntax(new TabSpaceSyntax(3), new SimpleSyntax("}},")));
            composite.Add(new ParagraphSyntax());

            MaskPart.Add(new LeftScopeSyntax(4));
            MaskPart.Add(maskBody);
            MaskPart.Add(new RightScopeSyntax(4));

            var maskSplitToArray = CalculateIndexesForMask(index, fieldCount);

            maskBody.Add(new TabSimpleSyntax(5, $"Index = {index},"));

            for (int i = 0; i < fieldCount; i++)
            {
                if (maskSplitToArray[fieldCount-1] > 1 && i < fieldCount-1)
                {
                    maskBody.Add(new CompositeSyntax(new TabSpaceSyntax(5), new SimpleSyntax($"Mask0{i + 1} = 1ul << {0},")));
                    maskBody.Add(new ParagraphSyntax());
                    continue;
                }
                
                maskBody.Add(new CompositeSyntax(new TabSpaceSyntax(5), new SimpleSyntax($"Mask0{i + 1} = 1ul << {maskSplitToArray[i]},")));

                if (i > fieldCount - 1)
                    continue;

                maskBody.Add(new ParagraphSyntax());
            }

            return composite;
        }

        public int[] CalculateIndexesForMask(int index, int fieldCounts)
        {
            var t = new List<int>(new int[fieldCounts + 1]);

            var calculate = index;

            for (int i = 0; i < fieldCounts; i++)
            {
                if (calculate + 2 > 63)
                {
                    t[i] = 63;
                    calculate -= 61;
                    continue;
                }

                if (calculate < 63 && calculate >= 0)
                {
                    t[i] = calculate + 2;
                    calculate -= 100;
                    continue;
                }

                else if (calculate < 0)
                {
                    t[i] = 0;
                }
            }

            return t.ToArray();
        }
        #endregion

        #region HECSMasks
        public string GenerateHecsMasks()
        {
            var tree = new TreeSyntaxNode();

            tree.Add(new NameSpaceSyntax("HECSFramework.Core"));
            tree.Add(new LeftScopeSyntax());
            tree.Add(new TabSimpleSyntax(1, "public static partial class HMasks"));
            tree.Add(new LeftScopeSyntax(1));
            tree.Add(GetHecsMasksFields());
            tree.Add(GetHecsMasksConstructor());
            tree.Add(new RightScopeSyntax(1));
            tree.Add(new RightScopeSyntax());

            return tree.ToString();
        }

        private ISyntax GetNewComponentSolved(Type c, int index, int fieldCount)
        {
            var tree = new TreeSyntaxNode();
            var maskBody = new TreeSyntaxNode();


            tree.Add(new ParagraphSyntax());
            tree.Add(new TabSimpleSyntax(4, $"new {typeof(HECSMask).Name}"));
            tree.Add(new LeftScopeSyntax(4));
            tree.Add(maskBody);
            tree.Add(new RightScopeSyntax(4, true));

            maskBody.Add(new TabSimpleSyntax(5, $"Index = {index},"));

            var maskSplitToArray = CalculateIndexesForMask(index, fieldCount);

            for (int i = 0; i < fieldCount; i++)
            {
                if (maskSplitToArray[fieldCount - 1] > 1 && i < fieldCount - 1)
                {
                    maskBody.Add(new CompositeSyntax(new TabSpaceSyntax(5), new SimpleSyntax($"Mask0{i + 1} = 1ul << {0},")));
                    maskBody.Add(new ParagraphSyntax());
                    continue;
                }

                maskBody.Add(new CompositeSyntax(new TabSpaceSyntax(5), new SimpleSyntax($"Mask0{i + 1} = 1ul << {maskSplitToArray[i]},")));

                if (i > fieldCount - 1)
                    continue;

                maskBody.Add(new ParagraphSyntax());
            }

            return tree;
        }

        private ISyntax GetHecsMasksConstructor()
        {
            var tree = new TreeSyntaxNode();
            tree.Add(new ParagraphSyntax());
            tree.Add(new TabSimpleSyntax(2, "static HMasks()"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(GetHMaskBody());
            tree.Add(new RightScopeSyntax(2));

            return tree;
        }

        private ISyntax GetHMaskBody()
        {
            var tree = new TreeSyntaxNode();

            for (int i = 0; i < componentTypes.Count; i++)
            {
                var className = componentTypes[i].Name.ToLower();
                var classType = componentTypes[i];
                var hash = IndexGenerator.GetIndexForType(classType);
                tree.Add(new TabSimpleSyntax(4, $"{className} = {GetNewComponentSolved(classType, i, ComponentsCount())}"));
            }

            return tree;
        }

        private string GetHECSMaskName()
        {
            return typeof(HECSMask).Name;
        }

        private ISyntax GetHecsMasksFields()
        {
            var tree = new TreeSyntaxNode();

            var hecsMaskname = typeof(HECSMask).Name;

            for (int i = 0; i < componentTypes.Count; i++)
            {
                tree.Add(new TabSimpleSyntax(2, $"private static {hecsMaskname} {componentTypes[i].Name.ToLower()};"));
                tree.Add(new TabSimpleSyntax(2, $"public static ref {hecsMaskname} {componentTypes[i].Name} => ref {componentTypes[i].Name.ToLower()};"));
            }

            return tree;
        }
        #endregion 

        #region ComponentContext
        public string GetComponentContext()
        {
            var overTree = new TreeSyntaxNode();
            var entityExtention = new TreeSyntaxNode();

            var usings = new TreeSyntaxNode();
            var nameSpaces = new List<string>();

            var tree = new TreeSyntaxNode();
            var properties = new TreeSyntaxNode();

            var disposable = new TreeSyntaxNode();
            var disposableBody = new TreeSyntaxNode();


            var switchAdd = new TreeSyntaxNode();
            var switchBody = new TreeSyntaxNode();

            var switchRemove = new TreeSyntaxNode();
            var switchRemoveBody = new TreeSyntaxNode();

            overTree.Add(tree);
            overTree.Add(entityExtention);

            tree.Add(usings);
            tree.Add(new ParagraphSyntax());

            tree.Add(new NameSpaceSyntax(DefaultNameSpace));
            tree.Add(new LeftScopeSyntax());

            tree.Add(new CompositeSyntax(new TabSpaceSyntax(1), new SimpleSyntax("public partial class ComponentContext : IDisposable"), new ParagraphSyntax()));
            tree.Add(new LeftScopeSyntax(1));
            tree.Add(properties);
            tree.Add(switchAdd);
            tree.Add(switchRemove);
            tree.Add(disposable);
            tree.Add(new RightScopeSyntax(1));
            tree.Add(new RightScopeSyntax());

            switchAdd.Add(new ParagraphSyntax());
            switchAdd.Add(new CompositeSyntax(new TabSpaceSyntax(2),
                new SimpleSyntax("partial void Add(IComponent component)"), new ParagraphSyntax()));
            switchAdd.Add(new LeftScopeSyntax(2));

            switchAdd.Add(new CompositeSyntax(new TabSpaceSyntax(3), new SimpleSyntax("switch (component)"), new ParagraphSyntax()));
            switchAdd.Add(new LeftScopeSyntax(3));
            switchAdd.Add(switchBody);
            switchAdd.Add(new RightScopeSyntax(3));
            switchAdd.Add(new RightScopeSyntax(2));

            switchAdd.Add(new ParagraphSyntax());
            switchAdd.Add(new CompositeSyntax(new TabSpaceSyntax(2),
                new SimpleSyntax("partial void Remove(IComponent component)"), new ParagraphSyntax()));
            switchAdd.Add(new LeftScopeSyntax(2));

            switchAdd.Add(new CompositeSyntax(new TabSpaceSyntax(3), new SimpleSyntax("switch (component)"), new ParagraphSyntax()));
            switchAdd.Add(new LeftScopeSyntax(3));
            switchAdd.Add(switchRemoveBody);
            switchAdd.Add(new RightScopeSyntax(3));
            switchAdd.Add(new RightScopeSyntax(2));

            foreach (var c in componentTypes)
            {
                properties.Add(new CompositeSyntax(new TabSpaceSyntax(2),
                    new SimpleSyntax($"public {c.Name} Get{c.Name} {{ get; private set; }}"), new ParagraphSyntax()));

                var cArgument = c.Name;
                var fixedArg = char.ToLower(cArgument[0]) + cArgument.Substring(1);

                switchBody.Add(new CompositeSyntax(new TabSpaceSyntax(4), new SimpleSyntax($"case {c.Name} {fixedArg}:"), new ParagraphSyntax()));
                switchBody.Add(new LeftScopeSyntax(5));
                switchBody.Add(new CompositeSyntax(new TabSpaceSyntax(6), new SimpleSyntax($"Get{c.Name} = {fixedArg};"), new ParagraphSyntax()));
                switchBody.Add(new CompositeSyntax(new TabSpaceSyntax(6), new ReturnSyntax()));
                switchBody.Add(new RightScopeSyntax(5));

                switchRemoveBody.Add(new CompositeSyntax(new TabSpaceSyntax(4), new SimpleSyntax($"case {c.Name} {fixedArg}:"), new ParagraphSyntax()));
                switchRemoveBody.Add(new LeftScopeSyntax(5));
                switchRemoveBody.Add(new CompositeSyntax(new TabSpaceSyntax(6), new SimpleSyntax($"Get{c.Name} = null;"), new ParagraphSyntax()));
                switchRemoveBody.Add(new CompositeSyntax(new TabSpaceSyntax(6), new ReturnSyntax()));
                switchRemoveBody.Add(new RightScopeSyntax(5));

                disposableBody.Add(new CompositeSyntax(new TabSpaceSyntax(3), new SimpleSyntax($"Get{c.Name} = null;"), new ParagraphSyntax()));
                //if (c != componentTypes.Last())
                //    switchBody.Add(new ParagraphSyntax());

                nameSpaces.AddOrRemoveElement(c.Namespace, true);
            }

            foreach (var n in nameSpaces)
                usings.Add(new UsingSyntax(n));

            AddEntityExtention(entityExtention);

            usings.Add(new UsingSyntax("System", 1));
            usings.Add(new UsingSyntax("System.Runtime.CompilerServices", 1));


            disposable.Add(new ParagraphSyntax());
            disposable.Add(new CompositeSyntax(new TabSpaceSyntax(2), new SimpleSyntax("public void Dispose()"), new ParagraphSyntax()));
            disposable.Add(new LeftScopeSyntax(2));
            disposable.Add(disposableBody);
            disposable.Add(new RightScopeSyntax(2));


            return overTree.ToString();
        }

        private void AddEntityExtention(TreeSyntaxNode tree)
        {
            var body = new TreeSyntaxNode();

            tree.Add(new ParagraphSyntax());

            tree.Add(new NameSpaceSyntax(DefaultNameSpace));
            tree.Add(new LeftScopeSyntax());

            tree.Add(new CompositeSyntax(new TabSpaceSyntax(1), new SimpleSyntax("public static class EntityComponentExtentions"), new ParagraphSyntax()));
            tree.Add(new LeftScopeSyntax(1));
            tree.Add(body);
            tree.Add(new RightScopeSyntax(1));
            tree.Add(new RightScopeSyntax());

            foreach (var c in componentTypes)
            {
                body.Add(new CompositeSyntax(new TabSpaceSyntax(2), new SimpleSyntax(AggressiveInline), new ParagraphSyntax()));
                body.Add(new CompositeSyntax(new TabSpaceSyntax(2),
                    new SimpleSyntax($"public static {c.Name} Get{c.Name}(this IEntity entity)"), new ParagraphSyntax()));
                body.Add(new LeftScopeSyntax(2));
                body.Add(new CompositeSyntax(new TabSpaceSyntax(3),
                    new SimpleSyntax($"return entity.ComponentContext.Get{c.Name};"), new ParagraphSyntax()));
                body.Add(new RightScopeSyntax(2));

                if (c != componentTypes.Last())
                    body.Add(new ParagraphSyntax());
            }
        }

        #endregion

        #region EntityExtentions
        private string GenerateEntityExtentions()
        {
            var tree = new TreeSyntaxNode();
            var methods = new TreeSyntaxNode();

            tree.Add(new UsingSyntax("Components"));
            tree.Add(new ParagraphSyntax());

            tree.Add(new UsingSyntax("System.Runtime.CompilerServices"));
            tree.Add(new ParagraphSyntax());
            tree.Add(new ParagraphSyntax());

            tree.Add(new NameSpaceSyntax(DefaultNameSpace));
            tree.Add(new LeftScopeSyntax());

            tree.Add(new CompositeSyntax(new TabSpaceSyntax(1), new SimpleSyntax("public static class EntityGenericExtentions"), new ParagraphSyntax()));

            tree.Add(new LeftScopeSyntax(1, true));
            tree.Add(methods);
            tree.Add(new RightScopeSyntax(1));
            tree.Add(new RightScopeSyntax(0));

            foreach (var c in componentTypes)
            {
                EntityAddComponent(c, methods);
                EntityGetComponent(c, methods);
                EntityRemoveComponent(c, methods);
            }

            return tree.ToString();
        }

        private void EntityAddComponent(Type component, TreeSyntaxNode tree)
        {
            tree.Add(new ParagraphSyntax());
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(2), new SimpleSyntax(AggressiveInline)));
            tree.Add(new ParagraphSyntax());

            tree.Add(new CompositeSyntax(new TabSpaceSyntax(2),
                new SimpleSyntax($"public static void Add{component.Name}(this Character entity, {component.Name} {component.Name.ToLower()}Component)")));

            tree.Add(new ParagraphSyntax());
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(3),
                new SimpleSyntax($"EntityManager.World(entity.WorldIndex).Add{component.Name}Component({component.Name.ToLower()}Component, ref entity);"),
                new ParagraphSyntax()));

            tree.Add(new RightScopeSyntax(2));
        }

        private void EntityGetComponent(Type component, TreeSyntaxNode tree)
        {
            tree.Add(new ParagraphSyntax());
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(2), new SimpleSyntax(AggressiveInline)));
            tree.Add(new ParagraphSyntax());

            tree.Add(new CompositeSyntax(new TabSpaceSyntax(2),
                new SimpleSyntax($"public static ref {component.Name} Get{component.Name}(this ref Character entity)")));

            tree.Add(new ParagraphSyntax());
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(3),
                new SimpleSyntax($"return ref EntityManager.World(entity.WorldIndex).Get{component.Name}Component(ref entity);"),
                new ParagraphSyntax()));

            tree.Add(new RightScopeSyntax(2));
        }

        private void EntityRemoveComponent(Type component, TreeSyntaxNode tree)
        {
            tree.Add(new ParagraphSyntax());
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(2), new SimpleSyntax(AggressiveInline)));
            tree.Add(new ParagraphSyntax());

            tree.Add(new CompositeSyntax(new TabSpaceSyntax(2),
                new SimpleSyntax($"public static void Remove{component.Name}(this ref Character entity)")));

            tree.Add(new ParagraphSyntax());
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(3),
                new SimpleSyntax($"EntityManager.World(entity.WorldIndex).Remove{component.Name}Component(ref entity);"),
                new ParagraphSyntax()));

            tree.Add(new RightScopeSyntax(2));
        }
        #endregion

        #region GenerateComponentMask
        public string GenerateMaskProvider()
        {
            var className = typeof(MaskProvider).Name;
            var hecsMaskname = typeof(HECSMask).Name;

            var hecsMaskPart = new TreeSyntaxNode();

            var componentsPeriodCount = ComponentsCount();


            //overallType
            var tree = new TreeSyntaxNode();

            //defaultMask
            var maskFunc = new TreeSyntaxNode();
            var maskDefault = new TreeSyntaxNode();

            var fields = new TreeSyntaxNode();
            var operatorPlus = new TreeSyntaxNode();
            var operatorMinus = new TreeSyntaxNode();
            var isHaveBody = new TreeSyntaxNode();

            var equalityBody = new TreeSyntaxNode();
            var getHashCodeBody = new TreeSyntaxNode();

            var maskClassConstructor = new TreeSyntaxNode();

            tree.Add(new NameSpaceSyntax(DefaultNameSpace));
            tree.Add(new LeftScopeSyntax());

            tree.Add(new TabSimpleSyntax(1, $"public partial class {className}"));
            tree.Add(new LeftScopeSyntax(1));

            //constructor
            tree.Add(new TabSimpleSyntax(2, "public MaskProvider()"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(maskClassConstructor);
            tree.Add(new RightScopeSyntax(2));

            //Get Empty Mask
            tree.Add(new ParagraphSyntax());
            tree.Add(new CompositeSyntax(maskFunc));
            maskFunc.Add(new TabSimpleSyntax(2, "public HECSMask GetEmptyMaskFunc()"));
            maskFunc.Add(new LeftScopeSyntax(2));
            maskFunc.Add(new TabSimpleSyntax(3, "return new HECSMask"));
            maskFunc.Add(new LeftScopeSyntax(3));
            maskFunc.Add(maskDefault);
            maskDefault.Add(new TabSimpleSyntax(4, "Index = -999,"));
            maskFunc.Add(new RightScopeSyntax(3, true));
            maskFunc.Add(new RightScopeSyntax(2));

            //plus operator
            tree.Add(new ParagraphSyntax());
            tree.Add(new CompositeSyntax(new TabSimpleSyntax(2, $"public {hecsMaskname} GetPlusFunc({hecsMaskname} l, {hecsMaskname} r)")));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(new CompositeSyntax(new TabSimpleSyntax(3, $"return new {hecsMaskname}")));
            tree.Add(new LeftScopeSyntax(3));
            tree.Add(operatorPlus);
            tree.Add(new RightScopeSyntax(3, true));
            tree.Add(new RightScopeSyntax(2));

            tree.Add(new ParagraphSyntax());
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(2), new SimpleSyntax($"public {hecsMaskname} GetMinusFunc({hecsMaskname} l, {hecsMaskname} r)"), new ParagraphSyntax()));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(3), new SimpleSyntax($"return new {hecsMaskname}"), new ParagraphSyntax()));
            tree.Add(new LeftScopeSyntax(3));
            tree.Add(operatorMinus);
            tree.Add(new RightScopeSyntax(3, true));
            tree.Add(new RightScopeSyntax(2));

            //Equal part
            tree.Add(new ParagraphSyntax());
            tree.Add(EqualMask(equalityBody));

            //HashCodePart part
            tree.Add(new ParagraphSyntax());
            tree.Add(GetHashCode(getHashCodeBody));

            //bool IsHave
            tree.Add(new ParagraphSyntax());
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(2), new SimpleSyntax($"public bool ContainsFunc(ref {hecsMaskname} original, ref {hecsMaskname} other)"), new ParagraphSyntax()));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(isHaveBody);
            tree.Add(new SimpleSyntax(CParse.Semicolon));
            isHaveBody.Add(new CompositeSyntax(new TabSpaceSyntax(3), new SimpleSyntax(CParse.Return), new SpaceSyntax()));
            tree.Add(new ParagraphSyntax());
            tree.Add(new RightScopeSyntax(2));
            tree.Add(new RightScopeSyntax(1));
            tree.Add(HecsMaskPart(hecsMaskPart));
            tree.Add(new RightScopeSyntax());

            //costructor for mask provider class
            maskClassConstructor.Add(GetMaskProviderConstructorBody());

            //fill trees
            for (int i = 0; i < ComponentsCount(); i++)
            {
                maskDefault.Add(new CompositeSyntax(new TabSpaceSyntax(4), new SimpleSyntax($"Mask0{i + 1} = 0,"), new ParagraphSyntax()));
                equalityBody.Add(new SimpleSyntax($"{CParse.Space}&& mask.Mask0{i + 1} == otherMask.Mask0{i + 1}"));
                fields.Add(new CompositeSyntax(new TabSpaceSyntax(2), new SimpleSyntax($"public ulong Mask0{i + 1};"), new ParagraphSyntax()));
                operatorPlus.Add(new CompositeSyntax(new TabSpaceSyntax(4), new SimpleSyntax($"Mask0{i + 1} = l.Mask0{i + 1} | r.Mask0{i + 1},"), new ParagraphSyntax()));
                operatorMinus.Add(new CompositeSyntax(new TabSpaceSyntax(4), new SimpleSyntax($"Mask0{i + 1} = l.Mask0{i + 1} ^ r.Mask0{i + 1},"), new ParagraphSyntax()));
                getHashCodeBody.Add(new TabSimpleSyntax(4, $"hash += ({(i+1)*3} * mask.Mask0{i + 1}.GetHashCode());"));

                if (i == 0)
                    isHaveBody.Add(new SimpleSyntax($"(original.Mask0{i + 1} & other.Mask0{i + 1}) != 0"));
                else
                    isHaveBody.Add(new CompositeSyntax(new ParagraphSyntax(), new TabSpaceSyntax(6),
                        new SimpleSyntax("&&"), new SimpleSyntax($"(original.Mask0{i + 1} & other.Mask0{i + 1}) != 0")));

                if (i > 0)
                    hecsMaskPart.Add(new TabSimpleSyntax(2, $"public ulong Mask0{i + 1};"));
            }

            return tree.ToString();
        }

        private ISyntax HecsMaskPart(ISyntax body)
        {
            var tree = new TreeSyntaxNode();
            var maskType = typeof(HECSMask).Name;

            //tree.Add(new NameSpaceSyntax("HECSFramework.Core"));
            //tree.Add(new LeftScopeSyntax());
            tree.Add(new ParagraphSyntax());
            tree.Add(new SimpleSyntax("#pragma warning disable" + CParse.Paragraph));
            tree.Add(new TabSimpleSyntax(1, $"public partial struct {maskType}"));
            tree.Add(new LeftScopeSyntax(1));
            tree.Add(body);
            tree.Add(new RightScopeSyntax(1));
            tree.Add(new SimpleSyntax("#pragma warning enable" + CParse.Paragraph));
            return tree;
        }

        private ISyntax GetHashCode(ISyntax body)
        {
            var tree = new TreeSyntaxNode();
            var maskType = typeof(HECSMask).Name;
            tree.Add(new TabSimpleSyntax(2, $"public int GetHashCodeFunc(ref {maskType} mask)"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(new TabSimpleSyntax(3, "unchecked"));
            tree.Add(new LeftScopeSyntax(3));
            tree.Add(new TabSimpleSyntax(4, "int hash = mask.Index;"));
            tree.Add(body);
            tree.Add(new TabSimpleSyntax(4, "return hash;"));
            tree.Add(new RightScopeSyntax(3));
            tree.Add(new RightScopeSyntax(2));
            return tree;
        }

        private ISyntax EqualMask(ISyntax body)
        {
            var tree = new TreeSyntaxNode();
            var maskSyntax = typeof(HECSMask).Name;

            tree.Add(new TabSimpleSyntax(2, $"public bool GetEqualityOfMasksFunc(ref {maskSyntax} mask, object other)"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(3), new SimpleSyntax($"return other is {maskSyntax} otherMask")));
            tree.Add(body);
            tree.Add(new SimpleSyntax(CParse.Semicolon));
            tree.Add(new ParagraphSyntax());
            tree.Add(new RightScopeSyntax(2));

            return tree;
        }

        private ISyntax GetMaskProviderConstructorBody()
        {
            var tree = new TreeSyntaxNode();
            tree.Add(new TabSimpleSyntax(3, "GetPlus = GetPlusFunc;"));
            tree.Add(new TabSimpleSyntax(3, "GetMinus = GetMinusFunc;"));
            tree.Add(new ParagraphSyntax());
            tree.Add(new TabSimpleSyntax(3, "Empty = GetEmptyMaskFunc;"));
            tree.Add(new TabSimpleSyntax(3, "Contains = ContainsFunc;"));
            tree.Add(new ParagraphSyntax());
            tree.Add(new TabSimpleSyntax(3, "GetMaskIsEqual = GetEqualityOfMasksFunc;"));
            tree.Add(new TabSimpleSyntax(3, "GetMaskHashCode = GetHashCodeFunc;"));

            return tree;
        }
        #endregion

        #region Helpers
        private int ComponentsCount()
        {
            double count = componentTypes.Count;

            if (count == 0)
                ++count;

            var componentsPeriodCount = Math.Ceiling(count / 61);

            return (int)componentsPeriodCount;
        }

        public void GatherAssembly()
        {
            var componentType = typeof(IComponent);
            var sysGlobalReactType = typeof(IGlobalCommand);
            var sysLocalReactType = typeof(ICommand);

            Assembly = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes());
            componentTypes = Assembly.Where(p => componentType.IsAssignableFrom(p) && !p.IsGenericType && !p.IsAbstract && !p.IsInterface).ToList();
            componentTypesByClass = Assembly.Where(p => p.Name != "BaseComponent" && componentType.IsAssignableFrom(p) && !p.IsAbstract && !p.IsGenericType && !p.IsInterface && p.IsSubclassOf(typeof(BaseComponent))).ToList();

            var localreact = typeof(IReactCommand<>);
            var globalReact = typeof(IReactGlobalCommand<>);
            var systemsTypes = typeof(ISystem);

            globalSystemBind = Assembly.Where(p => sysGlobalReactType.IsAssignableFrom(p) && !p.IsClass && !p.IsInterface).ToList();
            localSystemBind = Assembly.Where(p => sysLocalReactType.IsAssignableFrom(p) && !p.IsClass && !p.IsInterface).ToList();

            systems = Assembly.Where(p => systemsTypes.IsAssignableFrom(p) && p.IsClass && !p.IsInterface && !p.IsAbstract).ToList();
        }
        #endregion
    }
}