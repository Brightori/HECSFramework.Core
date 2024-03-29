using HECSFramework.Core;
using System;

namespace Components
{
    ///if u need add some project dependency to this class
    ///add partial part of this class to ur project, dont change this class
    ///if u need new functionality like add InetworkComponent interface - add them to part class

    [Serializable]
    [Documentation(Doc.HECS, Doc.GameLogic, "this component contains string and int id of container for entity, we can take container by this id from holders on client" )]
    public partial class ActorContainerID : BaseComponent, IDisposable
    {
        private string id;
        private int containerID = -1;

        public string ID
        {
            get => id;
            set => id = value;
        }
        
        public int ContainerIndex 
        {
            get
            {
                if (containerID == -1 && id != null)
                    containerID = IndexGenerator.GenerateIndex(id);

                return containerID;
            }
        }

        public void Dispose()
        {
            containerID = -1;
        }
    }

    public partial class ActorContainerID
    {

    }
}