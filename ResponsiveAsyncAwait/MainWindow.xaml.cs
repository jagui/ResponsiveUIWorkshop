using System;
using System.Collections.Generic;
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

namespace ResponsiveAsyncAwait
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



        private async void btnBurnClick(object sender, RoutedEventArgs e)
        {
            posts.ItemsSource = await BurnAsync(new[] { Common.Feeds.Digg, Common.Feeds.Meneame });
        }

        private async Task<IEnumerable<SyndicationItem>> BurnAsync(IEnumerable<String> feedUrls)
        {
            var items = new List<SyndicationItem>();
            foreach (var url in feedUrls)
            {
                var feed = await Task<XmlReader>.Factory.
                    StartNew(() => XmlReader.Create(url)).
                    ContinueWith(r => SyndicationFeed.Load(r.Result));
                items.AddRange(feed.Items);
            }
            return items;
        }

        private async void postSelected(object sender, SelectionChangedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                if (e.AddedItems != null && e.AddedItems.Count == 1)
                {
                    var selectedPost = (SyndicationItem)e.AddedItems[0];
                    var postContents = await GetPostContentsAsync(selectedPost);
                    webBrowser.NavigateToString(postContents);
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async Task<String> GetPostContentsAsync(SyndicationItem post)
        {
            var uri = new Uri(post.Id);
            var request = WebRequest.Create(uri);
            request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            var response = await request.GetResponseAsync();
            using (var responseStream = response.GetResponseStream())
            using (var reader = new StreamReader(responseStream))
            {
                return await reader.ReadToEndAsync();
            }
        }

    }
}

