namespace Deployer.Service.Contract.Helpers
{
    public interface IIISManager
    {
        string SurveyPath { get; }
        bool HasSurvey();
        void CreateSurvey(string surveyPath);
        void DeleteSurvey();
    }
}