using System;
using System.Collections.Generic;
using System.IO;
using Blacklist.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Blacklist
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasThumbImage, IHasWebPages
    {
        public static Plugin Instance { get; set; }
        public ImageFormat ThumbImageFormat => ImageFormat.Jpg;

        private readonly Guid id = new Guid("81C4097F-BE07-48B6-B76E-48691FDFC2C5");
        public override Guid Id => id;

        public override string Name => "Blacklist";
        
        public Stream GetThumbImage()
        {
            var type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".thumb.jpg");
        }

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(applicationPaths,
            xmlSerializer)
        {
            Instance = this;
        }

        public IEnumerable<PluginPageInfo> GetPages() => new[]
        {
            new PluginPageInfo
            {
                Name                 = "FirewallBanPluginConfigurationPage",
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.FirewallBanPluginConfigurationPage.html",
                DisplayName          = "Blacklist",
                EnableInMainMenu     = true

            },
            new PluginPageInfo
            {
                Name = "FirewallBanPluginConfigurationPageJS",
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.FirewallBanPluginConfigurationPage.js"
            }
        };
    }
}

