using Esri.ArcGISRuntime.Controls;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Esri.ArcGISRuntime.Portal;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.WebMap;
using System.Collections;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Http;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.Generic;
using System.Dynamic;
using Esri.ArcGISRuntime.Geometry;
using Newtonsoft.Json;

namespace AGOLCatPlusPlus
{

  public partial class MainWindow : Window
  {
    Esri.ArcGISRuntime.Portal.SearchResultInfo<Esri.ArcGISRuntime.Portal.ArcGISPortalItem> m_Results = null;
    private ArcGISPortal arcGISOnline;

    // address of the server hosting the secured content
    private const string PORTAL_URL = "https://www.arcgis.com/sharing/rest";
    private string _mptoken="";
    private string userName = "";
    private string password = "";
    private string portalURL = "https://esrinortheast.maps.arcgis.com";

    public MainWindow()
    {
      InitializeComponent();

      //this.DataContext = new MapViewModel();

      this.userName = Properties.Settings.Default.username;
      this.password = Properties.Settings.Default.password;
      this.portalURL = Properties.Settings.Default.portal;

      IdentityManager.Current.ChallengeHandler = new MyChallengeHandler();
      (this.DataContext as MapViewModel).MainMapView = this.MyMapView;//SBTEST
        

      //doSearch();
//      doLoadFromFile(null);
      doLoad();

    }
    private async Task doLoad()
    {

      await login();

      //JSONTextToDataTableTest(null);
      return;//SBTEST

      doLoadFromFile(null);

      
    }

    private async Task  login()
    {

      try
{
    // exception will be thrown here for bad credential ...
    var cred = await Esri.ArcGISRuntime.Security.IdentityManager.Current.GenerateCredentialAsync(
        PORTAL_URL, this.userName, this.password);

    // add the credential if it was generated successfully
    Esri.ArcGISRuntime.Security.IdentityManager.Current.AddCredential(cred);

    _mptoken = (IdentityManager.Current.Credentials.First() as TokenCredential).Token;

    // connecting to the portal will use an available credential (based on the server URL)
    var _portal = await Esri.ArcGISRuntime.Portal.ArcGISPortal.CreateAsync(new Uri(PORTAL_URL));
}
catch(ArcGISWebException webExp)
{
    var msg = "Could not log in. Please check credentials. Error code: " + webExp.Code;
MessageBox.Show(msg);

}
    }

    private DataTable JSONTextToDataTableTest(string sInput)
    {

      lookBusy();
      try
      {
        string sText = "";
        if (sInput == null)
        {
          using (StreamReader sr = new StreamReader("d:\\atemp\\jsoninput2.txt"))
          {
            sText = sr.ReadToEnd();
          }
        }
        else { sText = sInput; }

        //DataTable dt = JsonConvert.DeserializeObject<DataTable>(sText);
        dynamic input = JsonConvert.DeserializeObject(sText);
        PortalResultCollection pCachedData = new PortalResultCollection();


        if (input.items != null)
          foreach (var item in input.items)
          {
            MyArcGISPortalItem a = jsonToPortalItem(item);
            pCachedData.Add(a);
          }
        else if(input.results!=null)
        {
          foreach (var item in input.results)
          {
            MyArcGISPortalItem a = jsonToPortalItem(item);
            pCachedData.Add(a);
          }
        }
      



        this.lstItems.DataContext = pCachedData;
        dontLookBusy();
        return null;
      }
      catch(Exception ex)
      {
        string s = ex.Message;
      }
      dontLookBusy();
      return null;

    }

    private MyArcGISPortalItem jsonToPortalItem(dynamic item)
    {
      MyArcGISPortalItem p = new MyArcGISPortalItem();
      

      p.Description = item.description;
      p.Id = item.id;
      p.Name = item.name;
      p.Snippet = item.snippet;
      p.ThumbnailUri = "http://www.arcgis.com/sharing/rest/content/items/" + item.id + "/info/" + item.thumbnail;
      p.Title = item.title;
      p.Url = item.url;
      p.Type = item.type;

      return p;
    }

    private  void doLoadFromFile(string sFileInput)
    {

      try
      {

        string sFile=applicationDirectory + "\\org.csv";
        if (sFileInput != null) { sFile = sFileInput; }
        DataTable dt = CsvFileToDatatable(sFile, true);

        //System.Collections.ArrayList pCachedData = new System.Collections.ArrayList();
        PortalResultCollection pCachedData = new PortalResultCollection();

        foreach (DataRow r in dt.Rows)
        {
          DataRow rr = r;
          MyArcGISPortalItem a = dataRowToPortalItem(r);
          pCachedData.Add(a);
        }

        this.lstItems.DataContext = pCachedData;

        bool b = true;
       // string[] sResults = File.ReadAllLines(sFile);
      }
      catch(Exception ex)
      {
        string e = ex.Message;
        MessageBox.Show(e);
      }

    }

    private MyArcGISPortalItemCSV dataRowToPortalItemCSV(DataRow r)
    {
      MyArcGISPortalItemCSV pp = new MyArcGISPortalItemCSV();
      pp.name=  r["name"].ToString();
      pp.title = r["title"].ToString();
      pp.type = r["type"].ToString();
      pp.typeKeywords = r["typeKeywords"].ToString();
      pp.description = r["description"].ToString();
      pp.tags = r["tags"].ToString();
      pp.snippet = r["snippet"].ToString();
      pp.thumbnail = r["thumbnail"].ToString();
      pp.extent = r["extent"].ToString();
      pp.spatialReference = r["spatialReference"].ToString();
      pp.accessInformation = r["accessInformation"].ToString();
      pp.licenseInfo = r["licenseInfo"].ToString();
      pp.culture = r["culture"].ToString();
      pp.url = r["url"].ToString();
      pp.access = r["access"].ToString();
      pp.size = r["size"].ToString();
      pp.listed = r["listed"].ToString();
      pp.numComments = r["numComments"].ToString();
      pp.numRatings = r["numRatings"].ToString();
      pp.avgRatings = r["avgRatings"].ToString();
      pp.numViews = r["numViews"].ToString();
      pp.itemURL = r["itemURL"].ToString();



      return pp;
    }

    private MyArcGISPortalItem dataRowToPortalItem(DataRow r)
    {
      MyArcGISPortalItem pp = new MyArcGISPortalItem();
      pp.Title = r["name"].ToString();
      pp.Id= r["id"].ToString();
      pp.Title = r["title"].ToString();
      pp.Type = r["type"].ToString();
      pp.TypeKeywords= r["typeKeywords"].ToString();
      pp.Description = r["description"].ToString();
      pp.Tags = r["tags"].ToString();
      pp.Snippet = r["snippet"].ToString();
      pp.ThumbnailUri = r["thumbnail"].ToString();
      pp.ThumbnailUri = "http://www.arcgis.com/sharing/rest/content/items/" + r["id"] + "/info/" + r["thumbnail"];

      //sbtest:
      //pp.ThumbnailUri += pp.ThumbnailUri + "&token=" + _mptoken;

      pp.Extent = r["extent"].ToString();
      pp.SpatialReference = r["spatialReference"].ToString();
      pp.Access = r["accessInformation"].ToString();
      //pp.LicenseInfo = r["licenseInfo"].ToString();
      //pp.culture = r["culture"].ToString();
      pp.Url = r["url"].ToString();
      pp.Size = r["size"].ToString();
      
      pp.NumComments = r["numComments"].ToString();
      pp.NumRatings = r["numRatings"].ToString();
      pp.AvgRating = r["avgRatings"].ToString();
      pp.NumViews = r["numViews"].ToString();
      //pp.itemURL = r["itemURL"].ToString();



      return pp;
    }


    public DataTable CsvFileToDatatable(string path, bool IsFirstRowHeader)
    {
      string header = "No";
      string sql = string.Empty;
      DataTable dataTable = null;
      string pathOnly = string.Empty;
      string fileName = string.Empty;
      try
      {
        pathOnly = Path.GetDirectoryName(path);
        fileName = Path.GetFileName(path);
        sql = @"SELECT * FROM [" + fileName + "]";
        if (IsFirstRowHeader)
        {
          header = "Yes";
        }
        using (OleDbConnection connection = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathOnly +
        ";Extended Properties=\"Text;HDR=" + header + "\""))
        {
          using (OleDbCommand command = new OleDbCommand(sql, connection))
          {
            using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
            {
              dataTable = new DataTable();
              dataTable.Locale = CultureInfo.CurrentCulture;
              adapter.Fill(dataTable);
            }
          }
        }
      }
      catch(Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
      finally
      {
      }
      return dataTable;
    }

    private void lookBusy()
    {
      this.Cursor = Cursors.Wait;

    }
    private void dontLookBusy()
    {

      this.Cursor = Cursors.Arrow;
    }

    private async Task doSearch()
    {

      ArcGISPortal arcGISOnline = await Esri.ArcGISRuntime.Portal.ArcGISPortal.CreateAsync();

      var searchParams = new Esri.ArcGISRuntime.Portal.SearchParameters("\"map\" orgid:\"XWaQZrOGjgrsZ6Cu\" type:(\"web map\" NOT \"web mapping application\")");
      searchParams.Limit = 100;

      var itemSearch = await arcGISOnline.SearchItemsAsync(searchParams);
      var results = itemSearch.Results;

      this.lstItems.DataContext = results;

    }

    private void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
    {
      if (e.LoadError == null)
        return;

      Debug.WriteLine(string.Format("Error while loading layer : {0} - {1}", e.Layer.ID, e.LoadError.Message));
    }

    private void TextBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      Border_MouseLeftButtonDown(sender, e);
    }
    private async Task loadWebMap(string sID)
    {
      lookBusy();
      hideBrowser();
      var portalUri = new Uri("https://www.arcgis.com/sharing/rest");
      // create the portal 
      if (this.arcGISOnline == null) { this.arcGISOnline = await ArcGISPortal.CreateAsync(portalUri); }; 

      var searchParameters = new SearchParameters()
      {
          QueryString = "id: " + sID
      };

      var itemSearch = await this.arcGISOnline.SearchItemsAsync(searchParameters);
      var  results  = itemSearch.Results;

      if (itemSearch.TotalCount==1)
      {

        ArcGISPortalItem pItem = itemSearch.Results.First();// results[0];
        var webMap = await WebMap.FromPortalItemAsync(pItem);
        var webMapVM = await WebMapViewModel.LoadAsync(webMap, this.arcGISOnline);
        (this.DataContext as MapViewModel).MainMap = webMapVM.Map;

        dontLookBusy();
        foreach(Bookmark p in webMap.Bookmarks)
        {
          dynamic pp = new ExpandoObject();
          pp.Name=p.Name;
          SpatialReference sr=new SpatialReference(4236);
          if(p.Extent.SpatialReference!=null)sr=p.Extent.SpatialReference;

          Envelope ppp = new Envelope(p.Extent.XMin, p.Extent.YMin, p.Extent.XMax, p.Extent.YMax, sr);

          pp.extent = ppp;
          (this.DataContext as MapViewModel).WebMapBookmarks.Add(pp);
        }
        

        //add web bookmarks

        //this.MyMapView.Map = webMapVM.Map;

        hideBrowser();
        
      }
    

    }

    private void hideBrowser()
    {

      this.grdCatalog.Visibility = System.Windows.Visibility.Hidden;

    }
    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      MyArcGISPortalItem p = ((sender as FrameworkElement).DataContext as MyArcGISPortalItem);
      string sWebMapID = p.Id;


      string sDownload = string.Format("http://www.arcgis.com/sharing/rest/content/items/{0}/data", p.Id);

    
      if (p.Type.ToString().ToUpper() == "WEB MAP")
      {
        loadWebMap(sWebMapID);

      }
      else if (p.Type.ToString().ToUpper() == "WEB MAPPING APPLICATION")
      {

        string sUrl = p.Url;
        System.Diagnostics.Process.Start(sUrl);
      }
      else if (p.Type.ToString().ToUpper().IndexOf("PACKAGE") > 0)
      {
        //sDownload
       // HtmlPage.Window.Navigate(new Uri(sDownload), "_blank");
        if (p.Url != null)
        {
          string sUrl = p.Url;
          System.Diagnostics.Process.Start(sUrl);
        }
      }
      else if (p.Type.ToString().ToUpper().IndexOf("MAP SERVICE") > -1)
      {
        ArcGISTiledMapServiceLayer pTiled = new ArcGISTiledMapServiceLayer(new Uri(p.Url));
        
        this.MyMapView.Map.Layers.Add(pTiled);
        //this.MyMapView.SetView( pTiled.FullExtent);
        hideBrowser();
        
      }
      else if (p.Type.ToString().ToUpper().IndexOf("FEATURE SERVICE") > -1)
      {
        string sURL = p.Url;
        int r;
        if(!Int32.TryParse(sURL[sURL.Length-1].ToString(),out r)){
          sURL += "/0";
        }

        FeatureLayer pTiled = new FeatureLayer(new Uri(sURL));
        
        this.MyMapView.Map.Layers.Add(pTiled);
        //this.MyMapView.SetView( pTiled.FullExtent);
        hideBrowser();

      }
      else if (p.Url != null)
      {

        string sUrl = p.Url;
        System.Diagnostics.Process.Start(sUrl);
      }

    }

    private void Border_MouseLeftButton2(object sender, MouseButtonEventArgs e)
    {
      //ArcGISPortalItem p = ((sender as FrameworkElement).DataContext as ArcGISPortalItem);
      //string sWebMapID = p.Id;
      //string sDownload = string.Format("http://www.arcgis.com/sharing/rest/content/items/{0}/data", p.Id);

      //if (p.Type.ToString().ToUpper() == "WEBMAP")
      //{

      //  HtmlPage.Window.Navigate(new Uri("http://resources.arcgis.com/en/help/flex-viewer/live/index.html?itemid=" + p.Id + "&config=apps/basic-blackgold.xml"), "_blank");
      //}
      //else if (p.Type.ToString().ToUpper() == "WEBMAPPINGAPPLICATION")
      //{
      //  string sUrl = p.Url;
      //  HtmlPage.Window.Navigate(new Uri(sUrl), "_blank");
      //}
      //else if (p.Type.ToString().ToUpper().IndexOf("PACKAGE") > 0)
      //{
      //  //sDownload
      //  HtmlPage.Window.Navigate(new Uri(sDownload), "_blank");
      //}


    }


    private void Button_Click(object sender, RoutedEventArgs e)
    {
      if (grdCatalog.Visibility == Visibility.Hidden)
      {
        grdCatalog.Visibility = Visibility.Visible;
      }
      else
      {
        grdCatalog.Visibility = Visibility.Hidden;
      }
    }

    string applicationDirectory = (
from assembly in AppDomain.CurrentDomain.GetAssemblies()
where assembly.CodeBase.EndsWith(".exe")
select System.IO.Path.GetDirectoryName(assembly.CodeBase.Replace("file:///", ""))
).FirstOrDefault();

    private void MyMapView_DragOver(object sender, DragEventArgs e)
    {

    }

    private void MyMapView_Drop(object sender, DragEventArgs e)
    {

    }

    private void Grid_Drop(object sender, DragEventArgs e)
    {
      string[] docPath = (string[])e.Data.GetData(DataFormats.FileDrop);
      string sFile = docPath[0];
      doLoadFromFile(sFile);
    }

    private void cmdAGOLCat_Click(object sender, RoutedEventArgs e)
    {

      // the python program as a string. Note '@' which allow us to have a multiline string
//      String prg = @"import sys
//x = int(sys.argv[1])
//y = int(sys.argv[2])
//print x+y";
//      StreamWriter sw = new StreamWriter("d:\\atemp\\test2.py");
//      sw.Write(prg); // write this program to a file
//      sw.Close();


      string sFile = applicationDirectory + "\\searchResults.csv"; 
      sFile=sFile.Replace(@"\","/");
      if (File.Exists(sFile)) File.Delete(sFile);

      string sQuery = null;
      if (txtQuery.Text != null && txtQuery.Text != "") sQuery = txtQuery.Text;

      System.Diagnostics.Process p = new Process();
      p.StartInfo.FileName = "c:\\python27\\ArcGIS10.3\\python.exe";
      p.StartInfo.CreateNoWindow = false;
      p.StartInfo.UseShellExecute = false;
      p.StartInfo.RedirectStandardOutput = true;

      p.StartInfo.Arguments = "D:\\data\\_code\\_active\\agoTools\\samples\\AGOLCat.py -file " + sFile + " -u " + this.userName + " -p " + this.password +" -portal " +this.portalURL;
      if (sQuery != null) p.StartInfo.Arguments += " -q " + sQuery;

      int a = 2;
      int b = 2;
      //p.StartInfo.Arguments = "d:\\atemp\\test2.py " + a + " " + b; // start the python program with two parameters
            
      p.Start();
      p.WaitForExit();
      StreamReader s = p.StandardOutput;
      String output = s.ReadToEnd();
      //string sLog = p.StandardOutput.ReadToEnd();

      
      
      if (File.Exists(sFile))
        doLoadFromFile(sFile);

    }

    private void grdCatalog_KeyUp(object sender, KeyEventArgs e)
    {
      
    }

    private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {

      var text = Clipboard.GetData(DataFormats.Text) as string;
      JSONTextToDataTableTest(text);
      grdCatalog.Visibility = Visibility.Visible;
      e.Handled = true;

    }



  }

  public class PortalResultCollection : System.Collections.Generic.List <MyArcGISPortalItem>
  {
    public PortalResultCollection() { }
  }

  public class MyChallengeHandler:Esri.ArcGISRuntime.Security.IChallengeHandler
  {


    // address of the server hosting the secured content
    private const string PORTAL_URL = "https://www.arcgis.com/sharing/rest";

    // client id for your app (generated on developers.arcgis.com)
    // *** TODO: Replace CLIENT_ID with your arcgis.com App ID ***
    private const string CLIENT_ID = "2HEtx9ujil5rac8K";

    // address where the authorization response should be sent
    // for a server app, this might be the URL of a web service that can accept the authorization response
    // 'urn:ietf:wg:oauth:2.0:oob' means the authorization code will be sent as the title of a new page (and in the url querystring) 
    private const string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob";

    public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo requestInfo)
    {

      // see if information for this server already exists in the IdentityManager
      //SBTEST var serverInfo = IdentityManager.Current.FindServerInfo(SERVER_URL);
      var serverInfo = IdentityManager.Current.FindServerInfo(PORTAL_URL);

      // if info doesn't exist, register the server with the identity manager ...
      if (serverInfo == null)
      {
        serverInfo = new ServerInfo()
        {
          //            ServerUri = SERVER_URL,
          ServerUri = PORTAL_URL,
          TokenAuthenticationType = TokenAuthenticationType.OAuthAuthorizationCode,
          OAuthClientInfo = new OAuthClientInfo()
          {
            ClientId = CLIENT_ID,
            RedirectUri = REDIRECT_URI
          }
        };

        // add server info
        IdentityManager.Current.RegisterServer(serverInfo);

      }
      // get credentials for this server: this line will cause a challenge for username and password
      //SBTEST var cred = await IdentityManager.Current.GenerateCredentialAsync(SERVER_URL);
      var cred = await IdentityManager.Current.GenerateCredentialAsync(PORTAL_URL);


      // return the Esri.ArcGISRuntime.Security.Credential. If user was authorized, it will contain an access code 
      return cred;
    }
  }

  public class ImageBrushConverter : System.Windows.Data.IValueConverter
  {
    public ImageBrushConverter() { }


    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {


      BitmapImage _i = new BitmapImage();
      _i.BeginInit();

      if (value != null)
      {


        _i.UriSource = new Uri(value.ToString(), UriKind.Absolute);


        ImageBrush _b = new ImageBrush();
        if (_i != null && value != null && value.ToString() != "")
        {
          _b.ImageSource = _i;
          _b.Stretch = Stretch.UniformToFill;
        }
        _i.EndInit();
        return _b;
      }

      return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }

  public class MyArcGISPortalItemCSV
  {
    public string id { get; set; }
    public string owner { get; set; }
    public string created { get; set; }
    public string modified { get; set; }
    public string name { get; set; }
    public string title { get; set; }
    public string type { get; set; }
    public string typeKeywords { get; set; }
    public string description { get; set; }
    public string tags { get; set; }
    public string snippet { get; set; }
    public string thumbnail { get; set; }
    public string extent { get; set; }
    public string spatialReference { get; set; }
    public string accessInformation { get; set; }
    public string licenseInfo { get; set; }
    public string culture { get; set; }
    public string url { get; set; }
    public string access { get; set; }
    public string size { get; set; }
    public string listed { get; set; }
    public string numComments { get; set; }
    public string numRatings { get; set; }
    public string avgRatings { get; set; }
    public string numViews { get; set; }
    public string itemURL { get; set; }



  }

  public class MyArcGISPortalItem
  {
    public string Access { get; set; }
    public string AvgRating { get; set; }
    public string CreationDate { get; set; }
    public string Description { get; set; }
    public string Extent { get; set; }
    public string Id { get; set; }
    public string Name { get; set; }
    public string ModificationDate { get; set; }
    public string NumComments { get; set; }
    public string NumRatings { get; set; }
    public string NumViews { get; set; }
    public string Owner { get; set; }
    public string Size { get; set; }
    public string Snippet { get; set; }
    public string SpatialReference { get; set; }
    public string Tags { get; set; }
    public string ThumbnailUri { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
    public string TypeKeywords { get; set; }
    public string TypeName { get; set; }
    public string Url { get; set; }

  }

  public class ImageBrushConverter2 : System.Windows.Data.IValueConverter
  {
    public ImageBrushConverter2() { }


    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {

      //sbtest if (value == null || value == "") value = "Images/logo.png";
      if (value == null || value == "") value = null;

      BitmapImage _i = new BitmapImage();// (BitmapImage)value;

      _i.BeginInit();


      if (value != null)
      {

        //todo:
        if (value.ToString().ToUpper().IndexOf("CONFIG/") < 0 && value.ToString().ToUpper().IndexOf("LAYERIMAGES/") < 0 && value.ToString().ToUpper().IndexOf("ASSETS/") < 0)
        {
          //          _i.UriSource = new Uri(value.ToString(), UriKind.Absolute);
          _i.UriSource = new Uri(value.ToString(), UriKind.RelativeOrAbsolute);
        }
        else
        {
          _i.UriSource = new Uri(value.ToString(), UriKind.Relative);
        }

#if SILVERLIGHT

#else
        _i.EndInit();
#endif

        ImageBrush _b = new ImageBrush();
        if (_i != null && value != null && value.ToString() != "")
        {
          _b.ImageSource = _i;
          _b.Stretch = Stretch.Fill;
        }
        return _b;
      }

      return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }

  public class ImageBrushConverterAbsolute : System.Windows.Data.IValueConverter
  {

    public ImageBrushConverterAbsolute() { }

    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {

      BitmapImage _i = new BitmapImage();// (BitmapImage)value;

      _i.BeginInit();


      if (value != null)
      {

        //todo:
        if (value.ToString().ToUpper().IndexOf(";COMPONENT/") > 0)
        {
          _i.UriSource = new Uri(value.ToString(), UriKind.RelativeOrAbsolute);
        }
        else if (value.ToString().ToUpper().IndexOf("CONFIG/") < 0 && value.ToString().ToUpper().IndexOf("LAYERIMAGES/") < 0 && value.ToString().ToUpper().IndexOf("ASSETS/") < 0)
        {
          _i.UriSource = new Uri(value.ToString(), UriKind.Absolute);
        }
        else
        {
          _i.UriSource = new Uri(value.ToString(), UriKind.Relative);
        }


        _i.EndInit();

        ImageBrush _b = new ImageBrush();
        if (_i != null && value != null && value.ToString() != "")
        {
          _b.ImageSource = _i;
          _b.Stretch = Stretch.Fill;
        }
        return _b;
      }

      return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }

  public class VisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      bool visibility = (bool)value;
      return visibility ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      Visibility visibility = (Visibility)value;
      return (visibility == Visibility.Visible);
    }
  }


  public class BookmarksConfig : INotifyPropertyChanged
  {
   // public Envelope Extent = new Envelope();
    public string Description { get; set; }

   // public ESRI.ArcGIS.Client.Toolkit.Bookmark.MapBookmark _MapBookmark { get; set; }

    string layerTitle = "";
    public string Name
    {
      get { return layerTitle; }
      set
      {
        layerTitle = value;
        //if (_MapBookmark != null) { _MapBookmark.Name = LayerTitle; }
        NotifyPropertyChanged("Name");
      }
    }

    public string ThumbnailImageSourceURL { get; set; }
    public string SubDescription { get; set; }
    public string Tag { get; set; }

    bool isEditState = false;
    public bool IsEditState
    {
      get { return isEditState; }
      set { isEditState = value; NotifyPropertyChanged("IsEditState"); }
    }

    public bool NotIsEditState
    {
      get { return !isEditState; }
      set { isEditState = !value; NotifyPropertyChanged("NotIsEditState"); }
    }


    public event PropertyChangedEventHandler PropertyChanged;
    public void NotifyPropertyChanged(string propertyName)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this,
            new PropertyChangedEventArgs(propertyName));
      }
    }

  }

  public class BookmarksConfigCollection : List<Object>
  {
    public BookmarksConfigCollection() { }
  }

}
