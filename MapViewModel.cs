using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;

namespace AGOLCatPlusPlus
{
  class MapViewModel : System.ComponentModel.INotifyPropertyChanged
  {

    public MapViewModel()
    {
      this.map = App.Current.Resources["MainMap"] as Map;
      //this.webMapBookmarks = new System.Collections.ObjectModel.ObservableCollection<Esri.ArcGISRuntime.WebMap.Bookmark>();
      this.webMapBookmarks = new System.Collections.ObjectModel.ObservableCollection<ExpandoObject>();

      this.BookmarkChangedCommand = new DelegateCommand(BookmarkChanged);

     // Esri.ArcGISRuntime.WebMap.Bookmark bookm=new Esri.ArcGISRuntime.WebMap.Bookmark();
      //bookm.Name = "Test Me";
      //bookm.SetCustomProperty("IsEditState", false);
      //string s = bookm.GetCustomProperty("IsEditState").ToString();
      //this.webMapBookmarks.Add(bookm);
      
      Esri.ArcGISRuntime.Geometry.Envelope env = new Esri.ArcGISRuntime.Geometry.Envelope(-8804000,4737000,-7864000,5259000, new Esri.ArcGISRuntime.Geometry.SpatialReference(102100));
    
      dynamic myBookmark = new ExpandoObject();
      myBookmark.Name = "Test Me Too";
      myBookmark.IsEditState = false;
      myBookmark.extent = env;
      //this.webMapBookmarks.Add(myBookmark);

      dynamic myBookmark2 = new ExpandoObject();
      myBookmark2 = new ExpandoObject();
      myBookmark2.Name = "Test Me Three";
      myBookmark2.IsEditState = false;
      myBookmark2.extent = env;
      //this.webMapBookmarks.Add(myBookmark2);

    }

    private Map map;
    public Map MainMap
    {
      get { return this.map; }
      set { this.map = value; RaiseNotifyPropertyChanged("MainMap"); }
    }

    private MapView mapview;
    public MapView MainMapView
    {
      get { return this.mapview; }
      set { this.mapview = value; RaiseNotifyPropertyChanged("MainMapView"); }
    }

    public void BookmarkChanged(object parameter)
    {
      dynamic  e = parameter as ExpandoObject;
      Envelope env=e.extent;
      this.MainMapView.SetView(env);
      //MessageBox.Show("BookmarkChanged");

    }
    private System.Collections.ObjectModel.ObservableCollection<ExpandoObject> webMapBookmarks;
    public System.Collections.ObjectModel.ObservableCollection<ExpandoObject> WebMapBookmarks
    {

      get { return this.webMapBookmarks; }
      set { this.webMapBookmarks = value; RaiseNotifyPropertyChanged("WebMapBookmarks"); }

    }
    //private System.Collections.ObjectModel.ObservableCollection<Esri.ArcGISRuntime.WebMap.Bookmark> webMapBookmarks;
    //public System.Collections.ObjectModel.ObservableCollection<Esri.ArcGISRuntime.WebMap.Bookmark> WebMapBookmarks
    //{

    //  get { return this.webMapBookmarks; }
    //  set { this.webMapBookmarks = value; RaiseNotifyPropertyChanged("WebMapBookmarks"); }

    //}
    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    private void RaiseNotifyPropertyChanged(string propertyName = null)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
      }
    }

    DelegateCommand dg = null;
    public DelegateCommand BookmarkChangedCommand
    {
      get { return dg; }
      set { dg = value; }
    }

  }
}
