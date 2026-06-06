using System.Xml;
using UnityEngine;

namespace GanyuEditor.Extensions
{
    public static class XmlElementExtensions
    {
        public static void AppendField(this XmlElement self, string fieldName, Matrix4x4 value)
        {
            XmlElement matrixElement = self.OwnerDocument.CreateElement(fieldName);
            self.AppendChild(matrixElement);
            string text = string.Empty;
            text += "\n";
            for (int i = 0; i < 4; i++)
            {
                var row = value.GetRow(i);
                text += string.Format("{0,-10} {1,-10} {2,-10} {3,-10}\n", row.x, row.y, row.z, row.w);
            }
            matrixElement.InnerText = text;
        }

        public static void AppendField(this XmlElement self, string fieldName, Vector3 value)
        {
            XmlElement vectorElement = self.OwnerDocument.CreateElement(fieldName);
            self.AppendChild(vectorElement);
            vectorElement.InnerText = string.Format("{0,-10} {1,-10} {2,-10}", value.x, value.y, value.z);
        }

        public static void AppendField(this XmlElement self, string fieldName, float value)
        {
            XmlElement e = self.OwnerDocument.CreateElement(fieldName);
            self.AppendChild(e);
            e.InnerText = value.ToString();
        }

        public static void AppendField(this XmlElement self, string fieldName, int value)
        {
            XmlElement e = self.OwnerDocument.CreateElement(fieldName);
            self.AppendChild(e);
            e.InnerText = value.ToString();
        }
    }
}
