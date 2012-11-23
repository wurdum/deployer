namespace Deployer.Tests.Stubs
{
    public class SettingsFile
    {
        public static string Path {
            //get { return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings/DepSettings.xml"); }
            get { return @"stubs\DepSettings.xml"; }
        }
    }
}