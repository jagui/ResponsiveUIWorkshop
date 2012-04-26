using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace ResponsiveBackgroundWorker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            webBrowser.Navigated += (o, e) => Common.Browser.SuppressScriptErrors(webBrowser, true);
        }



        private void btnBurnClick(object sender, RoutedEventArgs e)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += (o, p) => p.Result = Burn((IEnumerable<string>)p.Argument);
            worker.RunWorkerCompleted += (o, r) => posts.ItemsSource = (IEnumerable<SyndicationItem>)r.Result;
            worker.RunWorkerAsync(new[] { Common.Feeds.Digg, Common.Feeds.Meneame });
        }

        private IEnumerable<SyndicationItem> Burn(IEnumerable<String> feedUrls)
        {
            var items = new List<SyndicationItem>();
            foreach (var url in feedUrls)
            {
                var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);
                items.AddRange(feed.Items);
            }
            return items;
        }

        private void postSelected(object sender, SelectionChangedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                if (e.AddedItems != null && e.AddedItems.Count == 1)
                {
                    var selectedPost = (SyndicationItem)e.AddedItems[0];
                    var worker = new BackgroundWorker();
                    worker.DoWork += (o, p) => p.Result = GetPostContents((SyndicationItem) p.Argument);
                    worker.RunWorkerCompleted += (o, r) => webBrowser.NavigateToString((string) r.Result);
                    worker.RunWorkerAsync(selectedPost);
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private String GetPostContents(SyndicationItem post)
        {
            var uri = new Uri(post.Id);
            var request = WebRequest.Create(uri);
            request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            var response = request.GetResponse();
            using (var responseStream = response.GetResponseStream())
            using (var reader = new StreamReader(responseStream))
            {
                return reader.ReadToEnd();
            }
        }

    }
}
