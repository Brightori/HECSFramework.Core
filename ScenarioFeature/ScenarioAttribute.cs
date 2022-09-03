using System;

namespace HECSFramework.Core
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class ScenarioAttribute : Attribute
    {
        public string ScenarioName;
        public float ScenarioStep;
        public string Comment;
        public ScenarioAttribute(string scenarioName, float scenarioStep, string comment)
        {
            ScenarioName = scenarioName;
            ScenarioStep = scenarioStep;
            Comment = comment;
        }
    }
}