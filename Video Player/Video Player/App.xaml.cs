using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Deployment;

namespace Video_Player
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var path = GetPath(e);
            if(path!=null)
            FileAssociationPath.setPath(path.ToString());
        }
        private static string GetPath(StartupEventArgs e)
        {
            if (e.Args.Length != 0)
                return e.Args[0];
            else
                return null;
        }
    }
}
