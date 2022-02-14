using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiPath.Studio.Activities.Api;
using UiPath.Studio.Activities.Api.Analyzer;
using UiPath.Studio.Activities.Api.Analyzer.Rules;
using UiPath.Studio.Analyzer.Models;

namespace WebinarLibrary
{
    public class RuleRepository2 : IRegisterAnalyzerConfiguration
    {
        public void Initialize(IAnalyzerConfigurationService workflowAnalyzerConfigService)
        {
            if (!workflowAnalyzerConfigService.HasFeature("WorkflowAnalyzerV4"))
                return;

            var checkIsThereLogMessageRule = new Rule<IWorkflowModel>("Check Log Message Exists", "TQ-INR-001", InspectWorkflowForLogMessage);
            checkIsThereLogMessageRule.RecommendationMessage = "Checking is the LogMessage activity created at beginning of every workflow in the project.";
            checkIsThereLogMessageRule.ErrorLevel = System.Diagnostics.TraceLevel.Warning;
            checkIsThereLogMessageRule.DefaultErrorLevel = System.Diagnostics.TraceLevel.Error;
            ((AnalyzerInspector)checkIsThereLogMessageRule).ApplicableScopes = new List<string>()
            {
                "DevelopmentRule"
            };
            workflowAnalyzerConfigService.AddRule<IWorkflowModel>(checkIsThereLogMessageRule);
        }

        private InspectionResult InspectWorkflowForLogMessage(IWorkflowModel workflowToInspect, UiPath.Studio.Activities.Api.Analyzer.Rules.Rule configuredRule)
        {
            List<string> stringList = new List<string>();
            if (!RuleRepository2.ContainLogMessage(workflowToInspect.Root))
                stringList.Add("LogMessage not used in first three activity in workflow" + ((IInspectionObject)workflowToInspect).DisplayName + ".");
            if (stringList.Count > 0)
                return new InspectionResult()
                {
                    ErrorLevel = configuredRule.ErrorLevel,
                    HasErrors = true,
                    RecommendationMessage = configuredRule.RecommendationMessage,
                    Messages = (ICollection<string>)stringList
                };
            return new InspectionResult() { HasErrors = false };
        }

        private static bool ContainLogMessage(IActivityModel activityModel)
        {
            bool flag = false;
            int counter = 0;
            if (activityModel.Children.Count == 0)
            {
                string[] strArray = activityModel.Type.Split(',');
                if (strArray.Length != 0 && strArray[0].Contains(".") && strArray[0].Substring(strArray[0].LastIndexOf('.') + 1).ToLower() == "logmessage")
                    flag = true;
            }
            else
            {
                string[] strArray = activityModel.Type.Split(',');
                if (strArray.Length != 0 && strArray[0].Contains("."))
                {
                    if (strArray[0].Substring(strArray[0].LastIndexOf('.') + 1).ToLower() == "commentout")
                    {
                        flag = false;
                    }
                    else
                    {
                        foreach (IActivityModel child in (IEnumerable<IActivityModel>)activityModel.Children)
                        {
                            counter++;
                            flag = RuleRepository2.ContainLogMessage(child);
                            if (flag || counter == 3)
                                break;
                        }
                    }
                }
            }
            return flag;
        }
    }
}
