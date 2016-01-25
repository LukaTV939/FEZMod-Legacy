﻿#pragma warning disable 436
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;
using System.Xml;
using FezEngine.Mod;
using MonoMod;
using Common;
using System;

namespace FezEngine.Tools {
    public class SharedContentManager {

        //non-existent in FEZ 1.11 for Windows; didn't test on Linux, but Mono didn't complain.
        [MonoModLinkTo(typeof(ContentManager), "Dispose")]
        public extern void Dispose();
        
        private class CommonContentManager {

            private extern T orig_ReadAsset<T>(string assetName) where T : class;
            private T ReadAsset<T>(string assetName) where T : class {
                if (assetName.Contains("-fm-")) {
                    assetName = assetName.Substring(0, assetName.IndexOf("-fm-"));
                }

                AssetMetadata metadata = null;
                //#if !FNA
                //Hey, we're in MonoGame! MonoGame suuuuuuuucks!
                //... does this syntax work?
                bool monogameSUUUUUUUUUUUUUUUUUUUUUCKS =
                    AssetMetadata.Map.TryGetValue(assetName, out metadata) ||
                    AssetMetadata.Map.TryGetValue(assetName + ".png", out metadata) ||
                    AssetMetadata.Map.TryGetValue(assetName + ".jpg", out metadata) ||
                    AssetMetadata.Map.TryGetValue(assetName + ".jpeg", out metadata) ||
                    AssetMetadata.Map.TryGetValue(assetName + ".gif", out metadata) ||
                    AssetMetadata.Map.TryGetValue(assetName + ".xml", out metadata);
                //#endif

                if (typeof(T) == typeof(Texture2D)) {
                    if (metadata == null) {
                        string imagePath = assetName.Externalize();
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
                            using (Stream s = new FileStream(imagePath + imageExtension, FileMode.Open)) {
                                return Texture2D.FromStream(ServiceHelper.Game.GraphicsDevice, s) as T;
                            }
                        }
                    } else {
                        using (Stream s = metadata.Assembly.GetManifestResourceStream(metadata.File)) {
                            return Texture2D.FromStream(ServiceHelper.Game.GraphicsDevice, s) as T;
                        }
                    }
                }

                string xmlPath = assetName.Externalize() + ".xml";
                if (File.Exists(xmlPath)) {
                    XmlDocument xmlDocument = null;
                    using (Stream s = metadata == null ? new FileStream(xmlPath, FileMode.Open) : metadata.Assembly.GetManifestResourceStream(metadata.File)) {
                        using (XmlReader xmlReader = XmlReader.Create(s)) {
                            xmlDocument = new XmlDocument();
                            xmlDocument.Load(xmlReader);
                        }
                    }
                    xmlDocument.DocumentElement.SetAttribute("assetName", assetName);

                    return xmlDocument.Deserialize(null, null, true) as T;
                }

                //Works in Mono, but .NET hates us.
                //.NET doesn't call the modified MemoryContentManager ReadAsset and directly
                //dies if something unexpected (for FEZ) happens.
                try {
                    return orig_ReadAsset<T>(assetName);
                } catch (Exception e) {
                    ModLogger.Log("FEZMod.Engine", "orig_ReadAsset failed on " + assetName + ": \n" + e);
                    return default(T);
                }
            }

        }

    }
}
