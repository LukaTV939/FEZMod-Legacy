﻿using System;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Xml;
using FezGame.Mod;
using Common;

namespace FezEngine.Tools {
    public class SharedContentManager {

        private class CommonContentManager {

            private T orig_ReadAsset<T>(string assetName) {
                return default(T);
            }

            private T ReadAsset<T>(string assetName) where T : class {
                if (typeof(T) == typeof(Texture2D)) {
                    string imagePath = MemoryContentManager.Externalize(assetName);
                    string imageExtension = null;

                    if (File.Exists(imagePath + ".png")) {
                        imageExtension = ".png";
                    } else if (File.Exists(imagePath + ".jpg")) {
                        imageExtension = ".jpg";
                    } else if (File.Exists(imagePath + ".jpeg")) {
                        imageExtension = ".jpeg";
                    } else if (File.Exists(imagePath + ".gif")) {
                        imageExtension = ".gif";
                    }

                    if (imageExtension != null) {
                        using (FileStream fs = new FileStream(imagePath + imageExtension, FileMode.Open)) {
                            return Texture2D.FromStream(ServiceHelper.Game.GraphicsDevice, fs) as T;
                        }
                    }
                }

                string xmlPath = MemoryContentManager.Externalize(assetName) + ".xml";
                if (File.Exists(xmlPath)) {
                    FileStream fis = new FileStream(xmlPath, FileMode.Open);
                    XmlReader xmlReader = XmlReader.Create(fis);
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(xmlReader);
                    xmlReader.Close();
                    fis.Close();

                    return xmlDocument.Deserialize(null, null, true) as T;
                }

                return orig_ReadAsset<T>(assetName);
            }

        }

    }
}

