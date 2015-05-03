﻿using System;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace FezGame.Editor {
    public static class EditorUtils {

        public static string ToString(this Vector2 v) {
            return v.X + ", " + v.Y;
        }

        public static string ToString(this Vector3 v) {
            return v.X + ", " + v.Y + ", " + v.Z;
        }

        public static T GetPrivate<T>(this object instance, string fieldName) {
            FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) {
                return default (T);
            }
            return (T) field.GetValue(instance);
        }

        public static bool Inside(this Vector2 point, Rectangle rectangle) {
            return
                rectangle.X <= point.X && point.X <= rectangle.X + rectangle.Width &&
                rectangle.Y <= point.Y && point.Y <= rectangle.Y + rectangle.Height;
        }

    }
}

