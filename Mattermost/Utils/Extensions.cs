using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Mattermost.Utils
{
    ///<summary>
    ///</summary>
    public static class UnixDateTimeHelper
    {
        private const string InvalidUnixEpochErrorMessage = "Unix epoc starts January 1st, 1970";
        /// <summary>
        ///   Convert a long into a DateTime
        /// </summary>
        public static DateTime FromUnixTime(this Int64 self)
        {
            var ret = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return ret.AddMilliseconds(self);
        }

        /// <summary>
        ///   Convert a DateTime into a long
        /// </summary>
        public static Int64 ToUnixTime(this DateTime self)
        {
            if (self == DateTime.MinValue)
            {
                return 0;
            }

            var epoc = new DateTime(1970, 1, 1);
            var delta = self - epoc;

            if (delta.TotalSeconds < 0) throw new ArgumentOutOfRangeException(InvalidUnixEpochErrorMessage);

            return (long)delta.TotalMilliseconds;
        }

        public static Task WaitOneAsync(this WaitHandle waitHandle)
        {
            if (waitHandle == null)
                throw new ArgumentNullException("waitHandle");

            var tcs = new TaskCompletionSource<bool>();
            var rwh = ThreadPool.RegisterWaitForSingleObject(waitHandle,
                delegate { tcs.TrySetResult(true); }, null, -1, true);
            var t = tcs.Task;
            t.ContinueWith((antecedent) => rwh.Unregister(null));
            return t;
        }

        public static Bitmap ApplyMask(this Bitmap input, Bitmap mask)
        {
            Bitmap output = new Bitmap(input.Width, input.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            output.MakeTransparent();
            var rect = new Rectangle(0, 0, input.Width, input.Height);

            var bitsMask = mask.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bitsInput = input.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bitsOutput = output.LockBits(rect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            unsafe
            {
                for (int y = 0; y < input.Height; y++)
                {
                    byte* ptrMask = (byte*)bitsMask.Scan0 + y * bitsMask.Stride;
                    byte* ptrInput = (byte*)bitsInput.Scan0 + y * bitsInput.Stride;
                    byte* ptrOutput = (byte*)bitsOutput.Scan0 + y * bitsOutput.Stride;
                    for (int x = 0; x < input.Width; x++)
                    {
                        //I think this is right - if the blue channel is 0 than all of them are (monochrome mask) which makes the mask black
                        if (ptrMask[4 * x] == 0)
                        {
                            ptrOutput[4 * x] = ptrInput[4 * x]; // blue
                            ptrOutput[4 * x + 1] = ptrInput[4 * x + 1]; // green
                            ptrOutput[4 * x + 2] = ptrInput[4 * x + 2]; // red

                            //Ensure opaque
                            ptrOutput[4 * x + 3] = 255;
                        }
                        else
                        {
                            ptrOutput[4 * x] = 0; // blue
                            ptrOutput[4 * x + 1] = 0; // green
                            ptrOutput[4 * x + 2] = 0; // red

                            //Ensure Transparent
                            ptrOutput[4 * x + 3] = 0; // alpha
                        }
                    }
                }
            }

            mask.UnlockBits(bitsMask);
            input.UnlockBits(bitsInput);
            output.UnlockBits(bitsOutput);

            return output;
        }

        public static T FindChild<T>(this DependencyObject parent, Predicate<T> filter = null) where T : DependencyObject
        {
            // If parent is null, just return null
            if (parent == null)
                return null;
            
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            // This queue is used to enable a breadth-first search on the children
            Queue<DependencyObject> children = new Queue<DependencyObject>();

            // Iterate over each child and search for a match
            for (int i = 0; i < childCount; i++)
            {
                // Add it to the queue
                children.Enqueue(VisualTreeHelper.GetChild(parent, i));
            }

            DependencyObject child;
            T childType;

            // If no direct child matched, 
            while (children.Count != 0)
            {
                child = children.Dequeue();
                childType = child as T;

                // If the child typecasts to the desired type, and passes the predicate, return it
                if (childType != null && filter(childType))
                    return childType;

                // Else, add its children to the queue and continue
                childCount = VisualTreeHelper.GetChildrenCount(child);

                for (int i = 0; i < childCount; i++)
                {
                    children.Enqueue(VisualTreeHelper.GetChild(child, i));
                }
            }

            return null;
        }
    }
}
