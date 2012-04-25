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

namespace ResponsiveAsynchronousDelegates
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
            var asyncBurn = new Func<IEnumerable<String>, IEnumerable<SyndicationItem>>((feeds) => Burn(feeds));
            asyncBurn.BeginInvoke(new[] { Common.Feeds.Digg, Common.Feeds.Meneame }, BurnEnd, asyncBurn);

        }

        private void BurnEnd(IAsyncResult ar)
        {
            var f = (Func<IEnumerable<String>, IEnumerable<SyndicationItem>>)ar.AsyncState;
            //posts.ItemsSource = f.EndInvoke(ar); //Cross threading exception
            Dispatcher.BeginInvoke(new Action(() => posts.ItemsSource = f.EndInvoke(ar))); //This one's ok
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

            if (e.AddedItems != null && e.AddedItems.Count == 1)
            {
                var selectedPost = (SyndicationItem)e.AddedItems[0];
                var asyncGetPostContents = new Func<SyndicationItem, String>((item) => GetPostContents(item));
                asyncGetPostContents.BeginInvoke(selectedPost, EndGetPostContents, asyncGetPostContents);
            }

        }

        private void EndGetPostContents(IAsyncResult ar)
        {
            var f = (Func<SyndicationItem, String>)ar.AsyncState;
            try
            {
                var postContents = f.EndInvoke(ar);
                //webBrowser.NavigateToString(postContents);//Cross threading exception
                Dispatcher.BeginInvoke(new Action(() => webBrowser.NavigateToString(postContents)));
            }
            finally
            {
                Dispatcher.BeginInvoke(new Action(() => Mouse.OverrideCursor = null));
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
