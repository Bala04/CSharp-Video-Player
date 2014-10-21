using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using SHDocVw;
namespace Video_Player
{
    /// <summary>
    /// Interaction logic for WordWindow.xaml
    /// </summary>
    public partial class WordWindow : Window
    {
        public WordWindow()
        {
            this.InitializeComponent();
        }
        public WordWindow(String word)
        {
           
            
        }
        public void getMeaning(String word)
        {
            //To get the meaning from google search
            WebRequest req = WebRequest.Create(new Uri("http://api.wordnik.com/v4/word.xml/" + word + "/definitions?api_key=526bcf3d25d108ee2e0080e2ca605d93e17b82f09807bc52d"));
            String content;
            using (StreamReader res = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                content = res.ReadToEnd();
            }
            StringBuilder output = new StringBuilder();
            output.Append("<html><body style=\"float:left;color:#363636\">");
            using (XmlReader reader = XmlReader.Create(new StringReader(content)))
            {
                XElement xelement = XElement.Load(reader, LoadOptions.SetBaseUri);
                IEnumerable<XElement> items = xelement.DescendantsAndSelf("definition");
                foreach (var Element in items)
                {
                    String temp = GetAttributeValue("sequence", Element);
                    if (temp.Contains("0"))
                    {
                        output.Append("<b style=\"font-size:14pt;font-family: Verdana,Arial,sans-serif;\">");
                        output.Append(GetElementValue("word", Element));
                        output.Append("</b><br/><ul style=\"list-style:none\">");
                    }
                    output.Append("<li>");
                    output.Append(GetElementValue("partOfSpeech", Element));
                    output.Append(" ");
                    output.Append(GetElementValue("text", Element));
                    output.Append("</li>");
                }
            }
            output.Append("</ul>");
            //output.Append("<script type=\"text/javascript\">( function() {if (window.CHITIKA === undefined) { window.CHITIKA = { 'units' : [] }; };var unit = {\"calltype\":\"async[2]\",\"publisher\":\"bala04\",\"width\":468,\"height\":60,\"sid\":\"Chitika Default\"}; var placement_id = window.CHITIKA.units.length;window.CHITIKA.units.push(unit); document.write('<div id=\"chitikaAdBlock-' + placement_id + '\"></div>');}());</script>");
            //output.Append("<script type=\"text/javascript\" src=\"//cdn.chitika.net/getads.js\" async></script>");
            output.Append("</body></html>");
            WebBrowserUI.NavigateToString(output.ToString());
           

        }
        private static string GetElementValue(string elementName, XElement element)
        {

            XElement Element = element.Element(elementName);
            String value = "";
            if (Element != null)
            {
                value = Element.Value;
            }
            if (value.Contains("adjective"))
                value = "adj.";
            return value;
        }

        private static string GetAttributeValue(string attributeName, XElement element)
        {
            XAttribute Attribute = element.Attribute(attributeName);

            String value = "";
            if (Attribute != null)
            {
                value = Attribute.Value;
            }

            return value;
        }
     }
}
