using HECSFramework.Core.Generator;
using HECSFramework.Core.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HECSFramework.Core
{
    public class CodeGenerator
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

        public void StartGeneration()
        {
            GatherAssembly();
            GenerateComponentMask();
            GenerateTypesMap();
            GenerateComponentContext();
        }


        #region GenerateTypesMap
        public string GenerateTypesMap()
        {
            var tree = new TreeSyntaxNode();
            var componentsSegment = new TreeSyntaxNode();

            tree.Add(new CompositeSyntax(new UsingSyntax("System.Collections.Generic"), new ParagraphSyntax()));
            tree.Add(new CompositeSyntax(new UsingSyntax("Components"), new ParagraphSyntax()));
            tree.Add(new CompositeSyntax(new UsingSyntax("HECSFramework.Core"), new ParagraphSyntax()));
            tree.Add(new ParagraphSyntax());
            tree.Add(new NameSpaceSyntax(DefaultNameSpace));
            tree.Add(new LeftScopeSyntax());
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(1), new SimpleSyntax($"public partial class {typeof(TypesProvider).Name}"), new ParagraphSyntax()));
            tree.Add(new LeftScopeSyntax(1));
            tree.Add(new TabSimpleSyntax(2, "public TypesProvider()"));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(new TabSimpleSyntax(3, $"Count = {componentTypes.Count + 1};"));
            tree.Add(new TabSimpleSyntax(3, $"MapIndexes = GetMapIndexes();"));
            tree.Add(new RightScopeSyntax(2));

            //size for componentsArray
            tree.Add(new ParagraphSyntax());


            //dictionaty
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(2), new SimpleSyntax("private Dictionary<int, ComponentMaskAndIndex> GetMapIndexes()")));
            tree.Add(new ParagraphSyntax());
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(new TabSimpleSyntax(3, "return new Dictionary<int, ComponentMaskAndIndex>"));
            tree.Add(new LeftScopeSyntax(3));
            tree.Add(componentsSegment);
            tree.Add(new RightScopeSyntax(3, true));
            tree.Add(new RightScopeSyntax(2));
            tree.Add(new RightScopeSyntax(1));
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

        private string GetComponentID(Type component)
        {
            //исключение, так как мы уже это везде используем, но не хочется получать IDID на конце
            if (component.Name.Contains("ActorContainerID"))
                return "ActorContainerID";

            return $"{component.Name}ID";
        }

        private ISyntax GetComponentForTypeMap(int index, int fieldCount, Type c)
        {
            var composite = new TreeSyntaxNode();
            var MaskPart = new TreeSyntaxNode();
            var maskBody = new TreeSyntaxNode();

            composite.Add(new ParagraphSyntax());
            composite.Add(new TabSpaceSyntax(3));
            composite.Add(new SimpleSyntax(CParse.LeftScope));
            composite.Add(new CompositeSyntax(new SimpleSyntax(IndexGenerator.GetIndexForType(c).ToString() + CParse.Comma)));
            composite.Add(new SimpleSyntax($"new ComponentMaskAndIndex {{ComponentName = {CParse.Quote}{ c.Name }{(CParse.Quote)}, ComponentsMask = new  {typeof(HECSMask).Name}"));
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
                    t[i] = 1;
                }
            }

            return t.ToArray();
        }
        #endregion


        #region ComponentContext
        private string GenerateComponentContext()
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
                new SimpleSyntax("public void AddComponent<T>(T component) where T: IComponent"), new ParagraphSyntax()));
            switchAdd.Add(new LeftScopeSyntax(2));

            switchAdd.Add(new CompositeSyntax(new TabSpaceSyntax(3), new SimpleSyntax("switch (component)"), new ParagraphSyntax()));
            switchAdd.Add(new LeftScopeSyntax(3));
            switchAdd.Add(switchBody);
            switchAdd.Add(new RightScopeSyntax(3));
            switchAdd.Add(new RightScopeSyntax(2));

            switchAdd.Add(new ParagraphSyntax());
            switchAdd.Add(new CompositeSyntax(new TabSpaceSyntax(2),
                new SimpleSyntax("public void RemoveComponent<T>(T component) where T: IComponent"), new ParagraphSyntax()));
            switchAdd.Add(new LeftScopeSyntax(2));

            switchAdd.Add(new CompositeSyntax(new TabSpaceSyntax(3), new SimpleSyntax("switch (component)"), new ParagraphSyntax()));
            switchAdd.Add(new LeftScopeSyntax(3));
            switchAdd.Add(switchRemoveBody);
            switchAdd.Add(new RightScopeSyntax(3));
            switchAdd.Add(new RightScopeSyntax(2));

            nameSpaces.Add("HECSFrameWork.Components");

            foreach (var c in componentTypes)
            {
                properties.Add(new CompositeSyntax(new TabSpaceSyntax(2),
                    new SimpleSyntax($"public {c.Name} Get{c.Name.Remove(0, 1)} {{ get; private set; }}"), new ParagraphSyntax()));

                var cArgument = c.Name.Remove(0, 1);
                var fixedArg = char.ToLower(cArgument[0]) + cArgument.Substring(1);

                switchBody.Add(new CompositeSyntax(new TabSpaceSyntax(4), new SimpleSyntax($"case {c.Name} {fixedArg}:"), new ParagraphSyntax()));
                switchBody.Add(new LeftScopeSyntax(5));
                switchBody.Add(new CompositeSyntax(new TabSpaceSyntax(6), new SimpleSyntax($"Get{c.Name.Remove(0, 1)} = {fixedArg};"), new ParagraphSyntax()));
                switchBody.Add(new CompositeSyntax(new TabSpaceSyntax(6), new ReturnSyntax()));
                switchBody.Add(new RightScopeSyntax(5));

                switchRemoveBody.Add(new CompositeSyntax(new TabSpaceSyntax(4), new SimpleSyntax($"case {c.Name} {fixedArg}:"), new ParagraphSyntax()));
                switchRemoveBody.Add(new LeftScopeSyntax(5));
                switchRemoveBody.Add(new CompositeSyntax(new TabSpaceSyntax(6), new SimpleSyntax($"Get{c.Name.Remove(0, 1)} = null;"), new ParagraphSyntax()));
                switchRemoveBody.Add(new CompositeSyntax(new TabSpaceSyntax(6), new ReturnSyntax()));
                switchRemoveBody.Add(new RightScopeSyntax(5));

                disposableBody.Add(new CompositeSyntax(new TabSpaceSyntax(3), new SimpleSyntax($"Get{c.Name.Remove(0, 1)} = null;"), new ParagraphSyntax()));
                //if (c != componentTypes.Last())
                //    switchBody.Add(new ParagraphSyntax());

                nameSpaces.AddOrRemoveElement(c.Namespace, true);
            }

            foreach (var n in nameSpaces)
            {
                usings.Add(new UsingSyntax(n));
                usings.Add(new ParagraphSyntax());
            }

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
                    new SimpleSyntax($"public static {c.Name} Get{c.Name.Remove(0, 1)}(this IEntity entity)"), new ParagraphSyntax()));
                body.Add(new LeftScopeSyntax(2));
                body.Add(new CompositeSyntax(new TabSpaceSyntax(3),
                    new SimpleSyntax($"return entity.ComponentContext.Get{c.Name.Remove(0, 1)};"), new ParagraphSyntax()));
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
                new SimpleSyntax($"public static void Add{component.Name}(this Entity entity, {component.Name} {component.Name.ToLower()}Component)")));

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
                new SimpleSyntax($"public static ref {component.Name} Get{component.Name}(this ref Entity entity)")));

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
                new SimpleSyntax($"public static void Remove{component.Name}(this ref Entity entity)")));

            tree.Add(new ParagraphSyntax());
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(3),
                new SimpleSyntax($"EntityManager.World(entity.WorldIndex).Remove{component.Name}Component(ref entity);"),
                new ParagraphSyntax()));

            tree.Add(new RightScopeSyntax(2));
        }


        #endregion

        #region GenerateComponentMask
        private string GenerateComponentMask()
        {
            var componentMaskName = "ComponentsMask";
            var componentsPeriodCount = ComponentsCount();


            //overallType
            var tree = new TreeSyntaxNode();

            //defaultMask
            var maskDefault = new TreeSyntaxNode();

            var fields = new TreeSyntaxNode();
            var operatorPlus = new TreeSyntaxNode();
            var operatorMinus = new TreeSyntaxNode();
            var isHaveBody = new TreeSyntaxNode();

            tree.FlatRoot.Add(new NameSpaceSyntax(DefaultNameSpace));
            tree.Add(new LeftScopeSyntax());

            //className
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(),
                new ModificatorSyntax(CParse.Public), new SimpleSyntax(CParse.Struct), new SimpleSyntax(componentMaskName), new ParagraphSyntax()));
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(), new LeftScopeSyntax()));
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(2), new SimpleSyntax("public static ComponentsMask Empty => new ComponentsMask"), new ParagraphSyntax()));
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(2), new LeftScopeSyntax()));

            //here we insert mask later
            tree.Add(new CompositeSyntax(maskDefault));
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(2), new RightScopeSyntax { StringValue = CParse.RightScope + CParse.Semicolon }));

            //here we insertFields
            tree.Add(new ParagraphSyntax());
            tree.Add(new CompositeSyntax(fields));
            tree.Add(new ParagraphSyntax());

            //plus operator
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(2), new SimpleSyntax("public static ComponentsMask operator +(ComponentsMask l, ComponentsMask r)"), new ParagraphSyntax()));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(3), new SimpleSyntax("return new ComponentsMask"), new ParagraphSyntax()));
            tree.Add(new LeftScopeSyntax(3));
            tree.Add(operatorPlus);
            tree.Add(new RightScopeSyntax(3, true));
            tree.Add(new RightScopeSyntax(2));

            tree.Add(new ParagraphSyntax());
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(2), new SimpleSyntax("public static ComponentsMask operator -(ComponentsMask l, ComponentsMask r)"), new ParagraphSyntax()));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(3), new SimpleSyntax("return new ComponentsMask"), new ParagraphSyntax()));
            tree.Add(new LeftScopeSyntax(3));
            tree.Add(operatorMinus);
            tree.Add(new RightScopeSyntax(3, true));
            tree.Add(new RightScopeSyntax(2));

            //bool IsHave
            tree.Add(new ParagraphSyntax());
            tree.Add(new CompositeSyntax(new TabSpaceSyntax(2), new SimpleSyntax("public bool IsHave(ref ComponentsMask mask)"), new ParagraphSyntax()));
            tree.Add(new LeftScopeSyntax(2));
            tree.Add(isHaveBody);
            isHaveBody.Add(new CompositeSyntax(new TabSpaceSyntax(3), new SimpleSyntax(CParse.Return), new SpaceSyntax()));
            tree.Add(new ParagraphSyntax());
            tree.Add(new RightScopeSyntax(2));
            tree.Add(new RightScopeSyntax(1));
            tree.Add(new RightScopeSyntax());

            //fill trees
            for (int i = 0; i < componentsPeriodCount; i++)
            {
                maskDefault.Add(new CompositeSyntax(new TabSpaceSyntax(3), new SimpleSyntax($"Mask0{i + 1} = 1 << 1,"), new ParagraphSyntax()));
                fields.Add(new CompositeSyntax(new TabSpaceSyntax(2), new SimpleSyntax($"public ulong Mask0{i + 1};"), new ParagraphSyntax()));
                operatorPlus.Add(new CompositeSyntax(new TabSpaceSyntax(4), new SimpleSyntax($"Mask0{i + 1} = l.Mask0{i + 1} | r.Mask0{i + 1},"), new ParagraphSyntax()));
                operatorMinus.Add(new CompositeSyntax(new TabSpaceSyntax(4), new SimpleSyntax($"Mask0{i + 1} = l.Mask0{i + 1} ^ r.Mask0{i + 1},"), new ParagraphSyntax()));

                if (i == 0)
                    isHaveBody.Add(new SimpleSyntax($"(Mask0{i + 1} & mask.Mask0{i + 1}) == mask.Mask0{i + 1}"));
                else
                    isHaveBody.Add(new CompositeSyntax(new ParagraphSyntax(), new TabSpaceSyntax(6), new SimpleSyntax("&&"), new SimpleSyntax($"(Mask0{i + 1} & mask.Mask0{i + 1}) == mask.Mask0{i + 1}")));
            }

            //add final semicolon
            isHaveBody.Add(new SimpleSyntax(CParse.Semicolon));

            var result = tree.ToString();
            return result;
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

            var asses = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes());
            componentTypes = asses.Where(p => componentType.IsAssignableFrom(p) && !p.IsGenericType && !p.IsAbstract && !p.IsInterface).ToList();

            componentTypesByClass = asses.Where(p => p.Name != "BaseComponent" && componentType.IsAssignableFrom(p) && !p.IsAbstract && !p.IsGenericType && !p.IsInterface && p.IsSubclassOf(typeof(BaseComponent))).ToList();
        }
        #endregion
    }
}