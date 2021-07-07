﻿/*
 *                     GNU AFFERO GENERAL PUBLIC LICENSE
 *                       Version 3, 19 November 2007
 *  Copyright (C) 2007 Free Software Foundation, Inc. <https://fsf.org/>
 *  Everyone is permitted to copy and distribute verbatim copies
 *  of this license document, but changing it is not allowed.
 */
// Port from: https://github.com/cyotek/Cyotek.Windows.Forms.ImageBox to AvaloniaUI
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Color = Avalonia.Media.Color;
using Pen = Avalonia.Media.Pen;
using Point = Avalonia.Point;
using Size = Avalonia.Size;

namespace UVtools.AvaloniaControls
{
    public class AdvancedImageBox : UserControl
    {
        #region Bindable Base
        /// <summary>
        ///     Multicast event for property change notifications.
        /// </summary>
        private PropertyChangedEventHandler _propertyChanged;

        public new event PropertyChangedEventHandler PropertyChanged
        {
            add => _propertyChanged += value;
            remove => _propertyChanged -= value;
        }
        protected bool RaiseAndSetIfChanged<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }


        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
        }

        /// <summary>
        ///     Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">
        ///     Name of the property used to notify listeners.  This
        ///     value is optional and can be provided automatically when invoked from compilers
        ///     that support <see cref="CallerMemberNameAttribute" />.
        /// </param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            OnPropertyChanged(e);
            _propertyChanged?.Invoke(this, e);
        }
        #endregion

        #region Sub Classes

        /// <summary>
        /// Represents available levels of zoom in an <see cref="ImageBox"/> control
        /// </summary>
        public class ZoomLevelCollection : IList<int>
        {
            #region Public Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="ZoomLevelCollection"/> class.
            /// </summary>
            public ZoomLevelCollection()
            {
                List = new SortedList<int, int>();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ZoomLevelCollection"/> class.
            /// </summary>
            /// <param name="collection">The default values to populate the collection with.</param>
            /// <exception cref="System.ArgumentNullException">Thrown if the <c>collection</c> parameter is null</exception>
            public ZoomLevelCollection(IEnumerable<int> collection)
              : this()
            {
                if (collection == null)
                {
                    throw new ArgumentNullException(nameof(collection));
                }

                AddRange(collection);
            }

            #endregion

            #region Public Class Properties

            /// <summary>
            /// Returns the default zoom levels
            /// </summary>
            public static ZoomLevelCollection Default =>
                new(new[] {
                    7, 10, 15, 20, 25, 30, 50, 70, 100, 150, 200, 300, 400, 500, 600, 700, 800, 1200, 1600, 3200
                });

            #endregion

            #region Public Properties

            /// <summary>
            /// Gets the number of elements contained in the <see cref="ZoomLevelCollection" />.
            /// </summary>
            /// <returns>
            /// The number of elements contained in the <see cref="ZoomLevelCollection" />.
            /// </returns>
            public int Count => List.Count;

            /// <summary>
            /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
            /// </summary>
            /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
            /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
            /// </returns>
            public bool IsReadOnly => false;

            /// <summary>
            /// Gets or sets the zoom level at the specified index.
            /// </summary>
            /// <param name="index">The index.</param>
            public int this[int index]
            {
                get => List.Values[index];
                set
                {
                    List.RemoveAt(index);
                    Add(value);
                }
            }

            #endregion

            #region Protected Properties

            /// <summary>
            /// Gets or sets the backing list.
            /// </summary>
            protected SortedList<int, int> List { get; set; }

            #endregion

            #region Public Members

            /// <summary>
            /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            public void Add(int item)
            {
                List.Add(item, item);
            }

            /// <summary>
            /// Adds a range of items to the <see cref="ZoomLevelCollection"/>.
            /// </summary>
            /// <param name="collection">The items to add to the collection.</param>
            /// <exception cref="System.ArgumentNullException">Thrown if the <c>collection</c> parameter is null.</exception>
            public void AddRange(IEnumerable<int> collection)
            {
                if (collection == null)
                {
                    throw new ArgumentNullException(nameof(collection));
                }

                foreach (int value in collection)
                {
                    Add(value);
                }
            }

            /// <summary>
            /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            public void Clear()
            {
                List.Clear();
            }

            /// <summary>
            /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
            /// </summary>
            /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
            public bool Contains(int item)
            {
                return List.ContainsKey(item);
            }

            /// <summary>
            /// Copies a range of elements this collection into a destination <see cref="Array"/>.
            /// </summary>
            /// <param name="array">The <see cref="Array"/> that receives the data.</param>
            /// <param name="arrayIndex">A 64-bit integer that represents the index in the <see cref="Array"/> at which storing begins.</param>
            public void CopyTo(int[] array, int arrayIndex)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[arrayIndex + i] = List.Values[i];
                }
            }

            /// <summary>
            /// Finds the index of a zoom level matching or nearest to the specified value.
            /// </summary>
            /// <param name="zoomLevel">The zoom level.</param>
            public int FindNearest(int zoomLevel)
            {
                int nearestValue = List.Values[0];
                int nearestDifference = Math.Abs(nearestValue - zoomLevel);
                for (int i = 1; i < Count; i++)
                {
                    int value = List.Values[i];
                    int difference = Math.Abs(value - zoomLevel);
                    if (difference < nearestDifference)
                    {
                        nearestValue = value;
                        nearestDifference = difference;
                    }
                }
                return nearestValue;
            }

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
            public IEnumerator<int> GetEnumerator()
            {
                return List.Values.GetEnumerator();
            }

            /// <summary>
            /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
            /// </summary>
            /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
            /// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
            public int IndexOf(int item)
            {
                return List.IndexOfKey(item);
            }

            /// <summary>
            /// Not implemented.
            /// </summary>
            /// <param name="index">The index.</param>
            /// <param name="item">The item.</param>
            /// <exception cref="System.NotImplementedException">Not implemented</exception>
            public void Insert(int index, int item)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Returns the next increased zoom level for the given current zoom.
            /// </summary>
            /// <param name="zoomLevel">The current zoom level.</param>
            /// <returns>The next matching increased zoom level for the given current zoom if applicable, otherwise the nearest zoom.</returns>
            public int NextZoom(int zoomLevel)
            {
                var index = IndexOf(FindNearest(zoomLevel));
                if (index < Count - 1)
                {
                    index++;
                }

                return this[index];
            }

            /// <summary>
            /// Returns the next decreased zoom level for the given current zoom.
            /// </summary>
            /// <param name="zoomLevel">The current zoom level.</param>
            /// <returns>The next matching decreased zoom level for the given current zoom if applicable, otherwise the nearest zoom.</returns>
            public int PreviousZoom(int zoomLevel)
            {
                var index = IndexOf(FindNearest(zoomLevel));
                if (index > 0)
                {
                    index--;
                }

                return this[index];
            }

            /// <summary>
            /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
            public bool Remove(int item)
            {
                return List.Remove(item);
            }

            /// <summary>
            /// Removes the element at the specified index of the <see cref="ZoomLevelCollection"/>.
            /// </summary>
            /// <param name="index">The zero-based index of the element to remove.</param>
            public void RemoveAt(int index)
            {
                List.RemoveAt(index);
            }

            /// <summary>
            /// Copies the elements of the <see cref="ZoomLevelCollection"/> to a new array.
            /// </summary>
            /// <returns>An array containing copies of the elements of the <see cref="ZoomLevelCollection"/>.</returns>
            public int[] ToArray()
            {
                int[] results;

                results = new int[Count];
                CopyTo(results, 0);

                return results;
            }

            #endregion

            #region IList<int> Members

            /// <summary>
            /// Returns an enumerator that iterates through a collection.
            /// </summary>
            /// <returns>An <see cref="ZoomLevelCollection" /> object that can be used to iterate through the collection.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        #endregion

        #region Enums

        /// <summary>
        /// Determines the sizing mode of an image hosted in an <see cref="UVtools.Avalonia.AdvancedImageBox" /> control.
        /// </summary>
        public enum SizeModes : byte
        {
            /// <summary>
            /// The image is displayed according to current zoom and scroll properties.
            /// </summary>
            Normal,

            /// <summary>
            /// The image is stretched to fill the client area of the control.
            /// </summary>
            Stretch,

            /// <summary>
            /// The image is stretched to fill as much of the client area of the control as possible, whilst retaining the same aspect ratio for the width and height.
            /// </summary>
            Fit
        }

        [Flags]
        public enum MouseButtons : byte
        {
            None = 0,
            LeftButton = 1,
            MiddleButton = 2,
            RightButton = 4
        }

        /// <summary>
        /// Describes the zoom action occuring
        /// </summary>
        [Flags]
        public enum ZoomActions : byte
        {
            /// <summary>
            /// No action.
            /// </summary>
            None = 0,

            /// <summary>
            /// The control is increasing the zoom.
            /// </summary>
            ZoomIn = 1,

            /// <summary>
            /// The control is decreasing the zoom.
            /// </summary>
            ZoomOut = 2,

            /// <summary>
            /// The control zoom was reset.
            /// </summary>
            ActualSize = 4
        }

        public enum SelectionModes
        {
            /// <summary>
            ///   No selection.
            /// </summary>
            None,

            /// <summary>
            ///   Rectangle selection.
            /// </summary>
            Rectangle,

            /// <summary>
            ///   Zoom selection.
            /// </summary>
            Zoom
        }

        #endregion

        #region UI Controls
        public ScrollBar HorizontalScrollBar { get; }
        public ScrollBar VerticalScrollBar { get; }
        public ContentPresenter ViewPort { get; }

        public Vector Offset
        {
            get => new(HorizontalScrollBar.Value, VerticalScrollBar.Value);
            set
            {
                HorizontalScrollBar.Value = value.X;
                VerticalScrollBar.Value = value.Y;
                RaisePropertyChanged();
                TriggerRender();
            }
        }

        public Size ViewPortSize => ViewPort.Bounds.Size;
        #endregion

        #region Private Members
        private Point _startMousePosition;
        private Vector _startScrollPosition;
        private bool _isPanning;
        private bool _isSelecting;
        private Bitmap _trackerImage;
        private bool _canRender = true;
        private Point _pointerPosition;
        #endregion

        #region Properties
        public static readonly DirectProperty<AdvancedImageBox, bool> CanRenderProperty =
            AvaloniaProperty.RegisterDirect<AdvancedImageBox, bool>(
                nameof(CanRender),
                o => o.CanRender);

        /// <summary>
        /// Gets or sets if control can render the image
        /// </summary>
        public bool CanRender
        {
            get => _canRender;
            set
            {
                if (!SetAndRaise(CanRenderProperty, ref _canRender, value)) return;
                if (_canRender) TriggerRender();
            }
        }

        public static readonly StyledProperty<byte> GridCellSizeProperty =
            AvaloniaProperty.Register<AdvancedImageBox, byte>(nameof(GridCellSize), 15);

        /// <summary>
        /// Gets or sets the grid cell size
        /// </summary>
        public byte GridCellSize
        {
            get => GetValue(GridCellSizeProperty);
            set => SetValue(GridCellSizeProperty, value);
        }

        public static readonly StyledProperty<ISolidColorBrush> GridColorProperty =
            AvaloniaProperty.Register<AdvancedImageBox, ISolidColorBrush>(nameof(GridColor), Brushes.Gainsboro);

        /// <summary>
        /// Gets or sets the color used to create the checkerboard style background
        /// </summary>
        public ISolidColorBrush GridColor
        {
            get => GetValue(GridColorProperty);
            set => SetValue(GridColorProperty, value);
        }

        public static readonly StyledProperty<ISolidColorBrush> GridColorAlternateProperty =
            AvaloniaProperty.Register<AdvancedImageBox, ISolidColorBrush>(nameof(GridColorAlternate), Brushes.White);

        /// <summary>
        /// Gets or sets the color used to create the checkerboard style background
        /// </summary>
        public ISolidColorBrush GridColorAlternate
        {
            get => GetValue(GridColorAlternateProperty);
            set => SetValue(GridColorAlternateProperty, value);
        }

        public static readonly StyledProperty<Bitmap> ImageProperty =
            AvaloniaProperty.Register<AdvancedImageBox, Bitmap>(nameof(Image));

        /// <summary>
        /// Gets or sets the image to be displayed
        /// </summary>
        public Bitmap Image
        {
            get => GetValue(ImageProperty);
            set
            {
                SetValue(ImageProperty, value);

                if (value is null)
                {
                    SelectNone();
                }

                UpdateViewPort();
                TriggerRender();

                RaisePropertyChanged(nameof(IsImageLoaded));
            }
        }

        public WriteableBitmap ImageAsWriteableBitmap => (WriteableBitmap) Image;

        public bool IsImageLoaded => Image is not null;

        public static readonly DirectProperty<AdvancedImageBox, Bitmap> TrackerImageProperty =
            AvaloniaProperty.RegisterDirect<AdvancedImageBox, Bitmap>(
                nameof(TrackerImage),
                o => o.TrackerImage,
                (o, v) => o.TrackerImage = v);

        /// <summary>
        /// Gets or sets an image to follow the mouse pointer
        /// </summary>
        public Bitmap TrackerImage
        {
            get => _trackerImage;
            set
            {
                if (!SetAndRaise(TrackerImageProperty, ref _trackerImage, value)) return;
                TriggerRender();
                RaisePropertyChanged(nameof(HaveTrackerImage));
            }
        }

        public bool HaveTrackerImage => _trackerImage is not null;

        public static readonly StyledProperty<bool> TrackerImageAutoZoomProperty =
            AvaloniaProperty.Register<AdvancedImageBox, bool>(nameof(TrackerImageAutoZoom), true);

        /// <summary>
        /// Gets or sets if the tracker image will be scaled to the current zoom
        /// </summary>
        public bool TrackerImageAutoZoom
        {
            get => GetValue(TrackerImageAutoZoomProperty);
            set => SetValue(TrackerImageAutoZoomProperty, value);
        }
        
        public bool IsHorizontalBarVisible
        {
            get
            {
                if (Image is null) return false;
                if (SizeMode != SizeModes.Normal) return false;
                return ScaledImageWidth > ViewPortSize.Width;
            }
        }

        public bool IsVerticalBarVisible
        {
            get
            {
                if (Image is null) return false;
                if (SizeMode != SizeModes.Normal) return false;
                return ScaledImageHeight > ViewPortSize.Height;
            }
        }

        public static readonly StyledProperty<bool> ShowGridProperty =
            AvaloniaProperty.Register<AdvancedImageBox, bool>(nameof(ShowGrid), true);

        /// <summary>
        /// Gets or sets the grid visibility when reach high zoom levels
        /// </summary>
        public bool ShowGrid
        {
            get => GetValue(ShowGridProperty);
            set => SetValue(ShowGridProperty, value);
        }

        public static readonly DirectProperty<AdvancedImageBox, Point> PointerPositionProperty =
            AvaloniaProperty.RegisterDirect<AdvancedImageBox, Point>(
                nameof(PointerPosition),
                o => o.PointerPosition);

        /// <summary>
        /// Gets the current pointer position
        /// </summary>
        public Point PointerPosition
        {
            get => _pointerPosition;
            private set => SetAndRaise(PointerPositionProperty, ref _pointerPosition, value);
        }

        public static readonly DirectProperty<AdvancedImageBox, bool> IsPanningProperty =
            AvaloniaProperty.RegisterDirect<AdvancedImageBox, bool>(
                nameof(IsPanning),
                o => o.IsPanning);

        /// <summary>
        /// Gets if control is currently panning
        /// </summary>
        public bool IsPanning
        {
            get => _isPanning;
            protected set
            {
                if (!SetAndRaise(IsPanningProperty, ref _isPanning, value)) return;
                _startScrollPosition = Offset;

                if (value)
                {
                    Cursor = new Cursor(StandardCursorType.SizeAll);
                    //this.OnPanStart(EventArgs.Empty);
                }
                else
                {
                    Cursor = Cursor.Default;
                    //this.OnPanEnd(EventArgs.Empty);
                }
            }
        }

        public static readonly DirectProperty<AdvancedImageBox, bool> IsSelectingProperty =
            AvaloniaProperty.RegisterDirect<AdvancedImageBox, bool>(
                nameof(IsSelecting),
                o => o.IsSelecting);

        /// <summary>
        /// Gets if control is currently selecting a ROI
        /// </summary>
        public bool IsSelecting
        {
            get => _isSelecting;
            protected set => SetAndRaise(IsSelectingProperty, ref _isSelecting, value);
        }

        /// <summary>
        /// Gets the center point of the viewport
        /// </summary>
        public Point CenterPoint
        {
            get
            {
                var viewport = GetImageViewPort();
                return new(viewport.Width / 2, viewport.Height / 2);
            }
        }

        public static readonly StyledProperty<bool> AutoPanProperty =
            AvaloniaProperty.Register<AdvancedImageBox, bool>(nameof(AutoPan), true);

        /// <summary>
        /// Gets or sets if the control can pan with the mouse
        /// </summary>
        public bool AutoPan
        {
            get => GetValue(AutoPanProperty);
            set => SetValue(AutoPanProperty, value);
        }

        public static readonly StyledProperty<MouseButtons> PanWithMouseButtonsProperty =
            AvaloniaProperty.Register<AdvancedImageBox, MouseButtons>(nameof(PanWithMouseButtons), MouseButtons.LeftButton | MouseButtons.MiddleButton | MouseButtons.RightButton);

        /// <summary>
        /// Gets or sets the mouse buttons to pan the image
        /// </summary>
        public MouseButtons PanWithMouseButtons
        {
            get => GetValue(PanWithMouseButtonsProperty);
            set => SetValue(PanWithMouseButtonsProperty, value);
        }

        public static readonly StyledProperty<bool> PanWithArrowsProperty =
            AvaloniaProperty.Register<AdvancedImageBox, bool>(nameof(PanWithArrows), true);

        /// <summary>
        /// Gets or sets if the control can pan with the keyboard arrows
        /// </summary>
        public bool PanWithArrows
        {
            get => GetValue(PanWithArrowsProperty);
            set => SetValue(PanWithArrowsProperty, value);
        }

        public static readonly StyledProperty<MouseButtons> SelectWithMouseButtonsProperty =
            AvaloniaProperty.Register<AdvancedImageBox, MouseButtons>(nameof(SelectWithMouseButtons), MouseButtons.LeftButton | MouseButtons.RightButton);


        /// <summary>
        /// Gets or sets the mouse buttons to select a region on image
        /// </summary>
        public MouseButtons SelectWithMouseButtons
        {
            get => GetValue(SelectWithMouseButtonsProperty);
            set => SetValue(SelectWithMouseButtonsProperty, value);
        }

        public static readonly StyledProperty<bool> InvertMousePanProperty =
            AvaloniaProperty.Register<AdvancedImageBox, bool>(nameof(InvertMousePan), false);

        /// <summary>
        /// Gets or sets if mouse pan is inverted
        /// </summary>
        public bool InvertMousePan
        {
            get => GetValue(InvertMousePanProperty);
            set => SetValue(InvertMousePanProperty, value);
        }

        public static readonly StyledProperty<bool> AutoCenterProperty =
            AvaloniaProperty.Register<AdvancedImageBox, bool>(nameof(AutoCenter), true);

        /// <summary>
        /// Gets or sets if image is auto centered
        /// </summary>
        public bool AutoCenter
        {
            get => GetValue(AutoCenterProperty);
            set => SetValue(AutoCenterProperty, value);
        }

        public static readonly StyledProperty<SizeModes> SizeModeProperty =
            AvaloniaProperty.Register<AdvancedImageBox, SizeModes>(nameof(SizeMode), SizeModes.Normal);

        /// <summary>
        /// Gets or sets the image size mode
        /// </summary>
        public SizeModes SizeMode
        {
            get => GetValue(SizeModeProperty);
            set
            {
                SetValue(SizeModeProperty, value);
                SizeModeChanged();
                RaisePropertyChanged(nameof(IsHorizontalBarVisible));
                RaisePropertyChanged(nameof(IsVerticalBarVisible));
            }
        }

        private void SizeModeChanged()
        {
            switch (SizeMode)
            {
                case SizeModes.Normal:
                    HorizontalScrollBar.Visibility = ScrollBarVisibility.Auto;
                    VerticalScrollBar.Visibility = ScrollBarVisibility.Auto;
                    break;
                case SizeModes.Stretch:
                case SizeModes.Fit:
                    HorizontalScrollBar.Visibility = ScrollBarVisibility.Hidden;
                    VerticalScrollBar.Visibility = ScrollBarVisibility.Hidden;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(SizeMode), SizeMode, null);
            }
        }

        public static readonly StyledProperty<bool> AllowZoomProperty =
            AvaloniaProperty.Register<AdvancedImageBox, bool>(nameof(AllowZoom), true);

        /// <summary>
        /// Gets or sets if zoom is allowed
        /// </summary>
        public bool AllowZoom
        {
            get => GetValue(AllowZoomProperty);
            set => SetValue(AllowZoomProperty, value);
        }

        public static readonly DirectProperty<AdvancedImageBox, ZoomLevelCollection> ZoomLevelsProperty =
            AvaloniaProperty.RegisterDirect<AdvancedImageBox, ZoomLevelCollection>(
                nameof(ZoomLevels),
                o => o.ZoomLevels,
                (o, v) => o.ZoomLevels = v);

        ZoomLevelCollection _zoomLevels = ZoomLevelCollection.Default;
        /// <summary>
        ///   Gets or sets the zoom levels.
        /// </summary>
        /// <value>The zoom levels.</value>
        public ZoomLevelCollection ZoomLevels
        {
            get => _zoomLevels;
            set => SetAndRaise(ZoomLevelsProperty, ref _zoomLevels, value);
        }

        public static readonly StyledProperty<int> MinZoomProperty =
            AvaloniaProperty.Register<AdvancedImageBox, int>(nameof(MinZoom), 10);

        /// <summary>
        /// Gets or sets the minimum possible zoom.
        /// </summary>
        /// <value>The zoom.</value>
        public int MinZoom
        {
            get => GetValue(MinZoomProperty);
            set => SetValue(MinZoomProperty, value);
        }

        public static readonly StyledProperty<int> MaxZoomProperty =
            AvaloniaProperty.Register<AdvancedImageBox, int>(nameof(MaxZoom), 3500);

        /// <summary>
        /// Gets or sets the maximum possible zoom.
        /// </summary>
        /// <value>The zoom.</value>
        public int MaxZoom
        {
            get => GetValue(MaxZoomProperty);
            set => SetValue(MaxZoomProperty, value);
        }


        public static readonly DirectProperty<AdvancedImageBox, int> OldZoomProperty =
            AvaloniaProperty.RegisterDirect<AdvancedImageBox, int>(
                nameof(OldZoom),
                o => o.OldZoom);

        private int _oldZoom = 100;

        /// <summary>
        /// Gets the previous zoom value
        /// </summary>
        /// <value>The zoom.</value>
        public int OldZoom
        {
            get => _oldZoom;
            private set => SetAndRaise(OldZoomProperty, ref _oldZoom, value);
        }

        public static readonly StyledProperty<int> ZoomProperty =
            AvaloniaProperty.Register<AdvancedImageBox, int>(nameof(Zoom), 100);

        /// <summary>
        ///  Gets or sets the zoom.
        /// </summary>
        /// <value>The zoom.</value>
        public int Zoom
        {
            get => GetValue(ZoomProperty);
            set
            {
                var newZoom = Math.Clamp(value, MinZoom, MaxZoom);

                var previousZoom = Zoom;
                if (previousZoom == newZoom) return;
                OldZoom = previousZoom;
                SetValue(ZoomProperty, value);

                UpdateViewPort();
                TriggerRender();

                RaisePropertyChanged(nameof(IsHorizontalBarVisible));
                RaisePropertyChanged(nameof(IsVerticalBarVisible));
            }
        }

        public bool IsActualSize => Zoom == 100;

        /// <summary>
        /// Gets the zoom factor, the zoom / 100
        /// </summary>
        public double ZoomFactor => Zoom / 100.0;

        /// <summary>
        /// Gets the width of the scaled image.
        /// </summary>
        /// <value>The width of the scaled image.</value>
        public double ScaledImageWidth => Image.Size.Width * ZoomFactor;

        /// <summary>
        /// Gets the height of the scaled image.
        /// </summary>
        /// <value>The height of the scaled image.</value>
        public double ScaledImageHeight => Image.Size.Height * ZoomFactor;

        public static readonly StyledProperty<ISolidColorBrush> PixelGridColorProperty =
            AvaloniaProperty.Register<AdvancedImageBox, ISolidColorBrush>(nameof(PixelGridColor), Brushes.DimGray);

        /// <summary>
        /// Gets or sets the color of the pixel grid.
        /// </summary>
        /// <value>The color of the pixel grid.</value>
        public ISolidColorBrush PixelGridColor
        {
            get => GetValue(PixelGridColorProperty);
            set => SetValue(PixelGridColorProperty, value);
        }

        public static readonly StyledProperty<int> PixelGridZoomThresholdProperty =
            AvaloniaProperty.Register<AdvancedImageBox, int>(nameof(PixelGridZoomThreshold), 5);

        /// <summary>
        /// Gets or sets the minimum size of zoomed pixel's before the pixel grid will be drawn
        /// </summary>
        /// <value>The pixel grid threshold.</value>

        public int PixelGridZoomThreshold
        {
            get => GetValue(PixelGridZoomThresholdProperty);
            set => SetValue(PixelGridZoomThresholdProperty, value);
        }

        public static readonly StyledProperty<SelectionModes> SelectionModeProperty =
            AvaloniaProperty.Register<AdvancedImageBox, SelectionModes>(nameof(SelectionMode), SelectionModes.None);

        public SelectionModes SelectionMode
        {
            get => GetValue(SelectionModeProperty);
            set => SetValue(SelectionModeProperty, value);
        }

        public static readonly StyledProperty<ISolidColorBrush> SelectionColorProperty =
            AvaloniaProperty.Register<AdvancedImageBox, ISolidColorBrush>(nameof(SelectionColor), new SolidColorBrush(new Color(127, 0, 128, 255)));

        public ISolidColorBrush SelectionColor
        {
            get => GetValue(SelectionColorProperty);
            set => SetValue(SelectionColorProperty, value);
        }

        public static readonly StyledProperty<Rect> SelectionRegionProperty =
            AvaloniaProperty.Register<AdvancedImageBox, Rect>(nameof(SelectionRegion), Rect.Empty);


        public Rect SelectionRegion
        {
            get => GetValue(SelectionRegionProperty);
            set
            {
                SetValue(SelectionRegionProperty, value);
                //if (!RaiseAndSetIfChanged(ref _selectionRegion, value)) return;
                TriggerRender();
                RaisePropertyChanged(nameof(HaveSelection));
                RaisePropertyChanged(nameof(SelectionRegionNet));
                RaisePropertyChanged(nameof(SelectionPixelSize));
            }
        }

        public Rectangle SelectionRegionNet
        {
            get
            {
                var rect = SelectionRegion;
                return new Rectangle((int) Math.Ceiling(rect.X), (int)Math.Ceiling(rect.Y),
                        (int)rect.Width, (int)rect.Height);
            }
        }

        public PixelSize SelectionPixelSize
        {
            get
            {
                var rect = SelectionRegion;
                return new PixelSize((int) rect.Width, (int) rect.Height);
            }
        }

        public bool HaveSelection => !SelectionRegion.IsEmpty;
        #endregion

        #region Constructor
        public AdvancedImageBox()
        {
            InitializeComponent();

            FocusableProperty.OverrideDefaultValue(typeof(AdvancedImageBox), true);
            AffectsRender<AdvancedImageBox>(ShowGridProperty);

            HorizontalScrollBar = this.FindControl<ScrollBar>("HorizontalScrollBar");
            VerticalScrollBar = this.FindControl<ScrollBar>("VerticalScrollBar");
            ViewPort = this.FindControl<ContentPresenter>("ViewPort");

            SizeModeChanged();

            HorizontalScrollBar.Scroll += ScrollBarOnScroll;
            VerticalScrollBar.Scroll += ScrollBarOnScroll;
            ViewPort.PointerWheelChanged += FillContainerOnPointerWheelChanged;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        #endregion

        #region Render methods
        public void TriggerRender(bool renderOnlyCursorTracker = false)
        {
            if (!_canRender) return;
            if (renderOnlyCursorTracker && _trackerImage is null) return;
                
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            //Debug.WriteLine($"Render: {DateTime.Now.Ticks}");
            base.Render(context);

            // Draw Grid
            var gridCellSize = GridCellSize;
            if (ShowGrid & gridCellSize > 0 && (!IsHorizontalBarVisible || !IsVerticalBarVisible))
            {
                // draw the background
                var gridColor = GridColor;
                var altColor = GridColorAlternate;
                var currentColor = gridColor;
                for (int y = 0; y < ViewPortSize.Height; y += gridCellSize)
                {
                    var firstRowColor = currentColor;

                    for (int x = 0; x < ViewPortSize.Width; x += gridCellSize)
                    {
                        context.FillRectangle(currentColor, new Rect(x, y, gridCellSize, gridCellSize));
                        currentColor = ReferenceEquals(currentColor, gridColor) ? altColor : gridColor;
                    }

                    if (Equals(firstRowColor, currentColor))
                        currentColor = ReferenceEquals(currentColor, gridColor) ? altColor : gridColor;
                }

            }
            /*else
            {
                context.FillRectangle(Background, new Rect(0, 0, Viewport.Width, Viewport.Height));
            }*/

            var image = Image;
            if (image is null) return;
            // Draw iamge
            context.DrawImage(image,
                GetSourceImageRegion(),
                GetImageViewPort()
            );

            var zoomFactor = ZoomFactor;


            if (HaveTrackerImage && _pointerPosition.X >= 0 && _pointerPosition.Y >= 0)
            {
                var destSize = TrackerImageAutoZoom
                    ? new Size(_trackerImage.Size.Width * zoomFactor, _trackerImage.Size.Height * zoomFactor)
                    : image.Size;

                var destPos = new Point(
                    _pointerPosition.X - destSize.Width / 2,
                    _pointerPosition.Y - destSize.Height / 2
                    );
                context.DrawImage(_trackerImage,
                    new Rect(destPos, destSize)
                );
            }

            //SkiaContext.SkCanvas.dr
            // Draw pixel grid
            if (zoomFactor > PixelGridZoomThreshold && SizeMode == SizeModes.Normal)
            {
                var viewport = GetImageViewPort();
                var offsetX = Offset.X % zoomFactor;
                var offsetY = Offset.Y % zoomFactor;

                Pen pen = new(PixelGridColor);
                for (double x = viewport.X + zoomFactor - offsetX; x < viewport.Right; x += zoomFactor)
                {
                    context.DrawLine(pen, new Point(x, viewport.X), new Point(x, viewport.Bottom));
                }

                for (double y = viewport.Y + zoomFactor - offsetY; y < viewport.Bottom; y += zoomFactor)
                {
                    context.DrawLine(pen, new Point(viewport.Y, y), new Point(viewport.Right, y));
                }

                context.DrawRectangle(pen, viewport);
            }

            if (!SelectionRegion.IsEmpty)
            {
                var rect = GetOffsetRectangle(SelectionRegion);
                var selectionColor = SelectionColor;
                context.FillRectangle(selectionColor, rect);
                Color color = Color.FromArgb(255, selectionColor.Color.R, selectionColor.Color.G, selectionColor.Color.B);
                context.DrawRectangle(new Pen(color.ToUint32()), rect);
            }
        }

        private bool UpdateViewPort()
        {
            if (Image is null)
            {
                HorizontalScrollBar.Maximum = 0;
                VerticalScrollBar.Maximum = 0;
                return true;
            }

            var scaledImageWidth = ScaledImageWidth;
            var scaledImageHeight = ScaledImageHeight;
            var width = scaledImageWidth - HorizontalScrollBar.ViewportSize;
            var height = scaledImageHeight - VerticalScrollBar.ViewportSize;
            //var width = scaledImageWidth <= Viewport.Width ? Viewport.Width : scaledImageWidth;
            //var height = scaledImageHeight <= Viewport.Height ? Viewport.Height : scaledImageHeight;

            bool changed = false;
            if (Math.Abs(HorizontalScrollBar.Maximum - width) > 0.01)
            {
                HorizontalScrollBar.Maximum = width;
                changed = true;
            }

            if (Math.Abs(VerticalScrollBar.Maximum - scaledImageHeight) > 0.01)
            {
                VerticalScrollBar.Maximum = height;
                changed = true;
            }

            /*if (changed)
            {
                var newContainer = new ContentControl
                {
                    Width = width,
                    Height = height
                };
                FillContainer.Content = SizedContainer = newContainer;
                Debug.WriteLine($"Updated ViewPort: {DateTime.Now.Ticks}");
                //TriggerRender();
            }*/

            return changed;
        }
        #endregion

        #region Events and Overrides

        private void ScrollBarOnScroll(object? sender, ScrollEventArgs e)
        {
            TriggerRender();
        }

        /*protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            Debug.WriteLine($"ViewportDelta: {e.ViewportDelta} | OffsetDelta: {e.OffsetDelta} | ExtentDelta: {e.ExtentDelta}");
            if (!e.ViewportDelta.IsDefault)
            {
                UpdateViewPort();
            }

            TriggerRender();

            base.OnScrollChanged(e);
        }*/

        private void FillContainerOnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            e.Handled = true;
            if (Image is null) return;
            if (AllowZoom && SizeMode == SizeModes.Normal)
            {
                // The MouseWheel event can contain multiple "spins" of the wheel so we need to adjust accordingly
                //double spins = Math.Abs(e.Delta.Y);
                //Debug.WriteLine(e.GetPosition(this));
                // TODO: Really should update the source method to handle multiple increments rather than calling it multiple times
                /*for (int i = 0; i < spins; i++)
                {*/
                ProcessMouseZoom(e.Delta.Y > 0, e.GetPosition(ViewPort));
                //}
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (e.Handled
                || _isPanning
                || _isSelecting
                || Image is null) return;

            var pointer = e.GetCurrentPoint(this);

            if (SelectionMode != SelectionModes.None)
            {
                if (!(
                        pointer.Properties.IsLeftButtonPressed && (SelectWithMouseButtons & MouseButtons.LeftButton) != 0 ||
                        pointer.Properties.IsMiddleButtonPressed && (SelectWithMouseButtons & MouseButtons.MiddleButton) != 0 ||
                        pointer.Properties.IsRightButtonPressed && (SelectWithMouseButtons & MouseButtons.RightButton) != 0
                    )
                ) return;
                IsSelecting = true;
            }
            else
            {
                if (!(
                        pointer.Properties.IsLeftButtonPressed && (PanWithMouseButtons & MouseButtons.LeftButton) != 0 ||
                        pointer.Properties.IsMiddleButtonPressed && (PanWithMouseButtons & MouseButtons.MiddleButton) != 0 ||
                        pointer.Properties.IsRightButtonPressed && (PanWithMouseButtons & MouseButtons.RightButton) != 0
                    )
                    || !AutoPan
                    || SizeMode != SizeModes.Normal

                ) return;

                IsPanning = true;
            }

            var location = pointer.Position;

            if (location.X > ViewPortSize.Width) return;
            if (location.Y > ViewPortSize.Height) return;
            _startMousePosition = location;
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (e.Handled) return;

            IsPanning = false;
            IsSelecting = false;
        }

        protected override void OnPointerLeave(PointerEventArgs e)
        {
            base.OnPointerLeave(e);
            PointerPosition = new Point(-1, -1);
            TriggerRender(true);
            e.Handled = true;
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (e.Handled) return;

            var pointer = e.GetCurrentPoint(this);
            PointerPosition = pointer.Position;

            if (!_isPanning && !_isSelecting)
            {
                TriggerRender(true);
                return;
            }

            if (_isPanning)
            {
                double x;
                double y;

                if (!InvertMousePan)
                {
                    x = _startScrollPosition.X + (_startMousePosition.X - _pointerPosition.X);
                    y = _startScrollPosition.Y + (_startMousePosition.Y - _pointerPosition.Y);
                }
                else
                {
                    x = (_startScrollPosition.X - (_startMousePosition.X - _pointerPosition.X));
                    y = (_startScrollPosition.Y - (_startMousePosition.Y - _pointerPosition.Y));
                }

                Offset = new Vector(x, y);
            }
            else if (_isSelecting)
            {
                double x;
                double y;
                double w;
                double h;

                var imageOffset = GetImageViewPort().Position;

                if (_pointerPosition.X < _startMousePosition.X)
                {
                    x = _pointerPosition.X;
                    w = _startMousePosition.X - _pointerPosition.X;
                }
                else
                {
                    x = _startMousePosition.X;
                    w = _pointerPosition.X - _startMousePosition.X;
                }

                if (_pointerPosition.Y < _startMousePosition.Y)
                {
                    y = _pointerPosition.Y;
                    h = _startMousePosition.Y - _pointerPosition.Y;
                }
                else
                {
                    y = _startMousePosition.Y;
                    h = _pointerPosition.Y - _startMousePosition.Y;
                }

                x -= imageOffset.X - Offset.X;
                y -= imageOffset.Y - Offset.Y;

                var zoomFactor = ZoomFactor;
                x /= zoomFactor;
                y /= zoomFactor;
                w /= zoomFactor;
                h /= zoomFactor;

                if (w != 0 && h != 0)
                {
                    SelectionRegion = FitRectangle(new Rect(x, y, w, h));
                }
            }

            e.Handled = true;
        }
        #endregion

        #region Zoom and Size modes
        private void ProcessMouseZoom(bool isZoomIn, Point cursorPosition)
         => PerformZoom(isZoomIn ? ZoomActions.ZoomIn : ZoomActions.ZoomOut, true, cursorPosition);

        /// <summary>
        /// Returns an appropriate zoom level based on the specified action, relative to the current zoom level.
        /// </summary>
        /// <param name="action">The action to determine the zoom level.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if an unsupported action is specified.</exception>
        private int GetZoomLevel(ZoomActions action)
        {
            var result = action switch
            {
                ZoomActions.None => Zoom,
                ZoomActions.ZoomIn => _zoomLevels.NextZoom(Zoom),
                ZoomActions.ZoomOut => _zoomLevels.PreviousZoom(Zoom),
                ZoomActions.ActualSize => 100,
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
            };
            return result;
        }

        /// <summary>
        /// Resets the <see cref="SizeModes"/> property whilsts retaining the original <see cref="Zoom"/>.
        /// </summary>
        protected void RestoreSizeMode()
        {
            if (SizeMode != SizeModes.Normal)
            {
                var previousZoom = Zoom;
                SizeMode = SizeModes.Normal;
                Zoom = previousZoom; // Stop the zoom getting reset to 100% before calculating the new zoom
            }
        }

        private void PerformZoom(ZoomActions action, bool preservePosition)
            => PerformZoom(action, preservePosition, CenterPoint);

        private void PerformZoom(ZoomActions action, bool preservePosition, Point relativePoint)
        {
            Point currentPixel = PointToImage(relativePoint);
            int currentZoom = Zoom;
            int newZoom = GetZoomLevel(action);

            /*if (preservePosition && Zoom != currentZoom)
                CanRender = false;*/

            RestoreSizeMode();
            Zoom = newZoom;

            if (preservePosition && Zoom != currentZoom)
            {
                ScrollTo(currentPixel, relativePoint);
            }
        }

        /// <summary>
        ///   Zooms into the image
        /// </summary>
        public void ZoomIn()
            => ZoomIn(true);

        /// <summary>
        ///   Zooms into the image
        /// </summary>
        /// <param name="preservePosition"><c>true</c> if the current scrolling position should be preserved relative to the new zoom level, <c>false</c> to reset.</param>
        public void ZoomIn(bool preservePosition)
        {
            PerformZoom(ZoomActions.ZoomIn, preservePosition);
        }

        /// <summary>
        ///   Zooms out of the image
        /// </summary>
        public void ZoomOut()
         => ZoomOut(true);

        /// <summary>
        ///   Zooms out of the image
        /// </summary>
        /// <param name="preservePosition"><c>true</c> if the current scrolling position should be preserved relative to the new zoom level, <c>false</c> to reset.</param>
        public void ZoomOut(bool preservePosition)
        {
            PerformZoom(ZoomActions.ZoomOut, preservePosition);
        }

        /// <summary>
        /// Zooms to the maximum size for displaying the entire image within the bounds of the control.
        /// </summary>
        public void ZoomToFit()
        {
            var image = Image;
            if (image is null) return;

            double zoom;
            double aspectRatio;

            if (image.Size.Width > image.Size.Height)
            {
                aspectRatio = ViewPortSize.Width / image.Size.Width;
                zoom = aspectRatio * 100.0;

                if (ViewPortSize.Height < image.Size.Height * zoom / 100.0)
                {
                    aspectRatio = ViewPortSize.Height / image.Size.Height;
                    zoom = aspectRatio * 100.0;
                }
            }
            else
            {
                aspectRatio = ViewPortSize.Height / image.Size.Height;
                zoom = aspectRatio * 100.0;

                if (ViewPortSize.Width < image.Size.Width * zoom / 100.0)
                {
                    aspectRatio = ViewPortSize.Width / image.Size.Width;
                    zoom = aspectRatio * 100.0;
                }
            }

            Zoom = (int)zoom;
        }

        /// <summary>
        ///   Adjusts the view port to fit the given region
        /// </summary>
        /// <param name="x">The X co-ordinate of the selection region.</param>
        /// <param name="y">The Y co-ordinate of the selection region.</param>
        /// <param name="width">The width of the selection region.</param>
        /// <param name="height">The height of the selection region.</param>
        /// <param name="margin">Give a margin to rectangle by a value to zoom-out that pixel value</param>
        public void ZoomToRegion(double x, double y, double width, double height, double margin = 0)
        {
            ZoomToRegion(new Rect(x, y, width, height), margin);
        }

        /// <summary>
        ///   Adjusts the view port to fit the given region
        /// </summary>
        /// <param name="x">The X co-ordinate of the selection region.</param>
        /// <param name="y">The Y co-ordinate of the selection region.</param>
        /// <param name="width">The width of the selection region.</param>
        /// <param name="height">The height of the selection region.</param>
        /// <param name="margin">Give a margin to rectangle by a value to zoom-out that pixel value</param>
        public void ZoomToRegion(int x, int y, int width, int height, double margin = 0)
        {
            ZoomToRegion(new Rect(x, y, width, height), margin);
        }

        /// <summary>
        ///   Adjusts the view port to fit the given region
        /// </summary>
        /// <param name="rectangle">The rectangle to fit the view port to.</param>
        /// <param name="margin">Give a margin to rectangle by a value to zoom-out that pixel value</param>
        public void ZoomToRegion(Rectangle rectangle, double margin = 0) =>
            ZoomToRegion(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, margin);

        /// <summary>
        ///   Adjusts the view port to fit the given region
        /// </summary>
        /// <param name="rectangle">The rectangle to fit the view port to.</param>
        /// <param name="margin">Give a margin to rectangle by a value to zoom-out that pixel value</param>
        public void ZoomToRegion(Rect rectangle, double margin = 0)
        {
            if (margin > 0) rectangle = rectangle.Inflate(margin);
            var ratioX = ViewPortSize.Width / rectangle.Width;
            var ratioY = ViewPortSize.Height / rectangle.Height;
            var zoomFactor = Math.Min(ratioX, ratioY);
            var cx = rectangle.X + rectangle.Width / 2;
            var cy = rectangle.Y + rectangle.Height / 2;

            CanRender = false;
            Zoom = (int)(zoomFactor * 100); // This function sets the zoom so viewport will change
            CenterAt(new Point(cx, cy)); // If i call this here, it will move to the wrong position due wrong viewport
        }

        /// <summary>
        /// Zooms to current selection region
        /// </summary>
        public void ZoomToSelectionRegion(double margin = 0)
        {
            if (!HaveSelection) return;
            ZoomToRegion(SelectionRegion, margin);
        }

        /// <summary>
        /// Resets the zoom to 100%.
        /// </summary>
        public void PerformActualSize()
        {
            SizeMode = SizeModes.Normal;
            //SetZoom(100, ImageZoomActions.ActualSize | (Zoom < 100 ? ImageZoomActions.ZoomIn : ImageZoomActions.ZoomOut));
            Zoom = 100;
        }
        #endregion

        #region Utility methods
        /// <summary>
        ///   Determines whether the specified point is located within the image view port
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>
        ///   <c>true</c> if the specified point is located within the image view port; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPointInImage(Point point)
            => GetImageViewPort().Contains(point);

        /// <summary>
        ///   Determines whether the specified point is located within the image view port
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to check.</param>
        /// <param name="y">The Y co-ordinate of the point to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified point is located within the image view port; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPointInImage(int x, int y)
            => IsPointInImage(new Point(x, y));

        /// <summary>
        ///   Determines whether the specified point is located within the image view port
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to check.</param>
        /// <param name="y">The Y co-ordinate of the point to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified point is located within the image view port; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPointInImage(double x, double y)
            => IsPointInImage(new Point(x, y));

        /// <summary>
        ///   Converts the given client size point to represent a coordinate on the source image.
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to convert.</param>
        /// <param name="y">The Y co-ordinate of the point to convert.</param>
        /// <param name="fitToBounds">
        ///   if set to <c>true</c> and the point is outside the bounds of the source image, it will be mapped to the nearest edge.
        /// </param>
        /// <returns><c>Point.Empty</c> if the point could not be matched to the source image, otherwise the new translated point</returns>
        public Point PointToImage(double x, double y, bool fitToBounds = true)
            => PointToImage(new Point(x, y), fitToBounds);

        /// <summary>
        ///   Converts the given client size point to represent a coordinate on the source image.
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to convert.</param>
        /// <param name="y">The Y co-ordinate of the point to convert.</param>
        /// <param name="fitToBounds">
        ///   if set to <c>true</c> and the point is outside the bounds of the source image, it will be mapped to the nearest edge.
        /// </param>
        /// <returns><c>Point.Empty</c> if the point could not be matched to the source image, otherwise the new translated point</returns>
        public Point PointToImage(int x, int y, bool fitToBounds = true)
        {
            return PointToImage(new Point(x, y), fitToBounds);
        }

        /// <summary>
        ///   Converts the given client size point to represent a coordinate on the source image.
        /// </summary>
        /// <param name="point">The source point.</param>
        /// <param name="fitToBounds">
        ///   if set to <c>true</c> and the point is outside the bounds of the source image, it will be mapped to the nearest edge.
        /// </param>
        /// <returns><c>Point.Empty</c> if the point could not be matched to the source image, otherwise the new translated point</returns>
        public Point PointToImage(Point point, bool fitToBounds = true)
        {
            double x;
            double y;

            var viewport = GetImageViewPort();

            if (!fitToBounds || viewport.Contains(point))
            {
                x = (point.X + Offset.X - viewport.X) / ZoomFactor;
                y = (point.Y + Offset.Y - viewport.Y) / ZoomFactor;

                var image = Image;
                if (fitToBounds)
                {
                    x = Math.Clamp(x, 0, image.Size.Width-1);
                    y = Math.Clamp(y, 0, image.Size.Height-1);
                }
            }
            else
            {
                x = 0; // Return Point.Empty if we couldn't match
                y = 0;
            }

            return new(x, y);
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.Point" /> repositioned to include the current image offset and scaled by the current zoom level
        /// </summary>
        /// <param name="source">The source <see cref="Point"/> to offset.</param>
        /// <returns>A <see cref="Point"/> which has been repositioned to match the current zoom level and image offset</returns>
        public Point GetOffsetPoint(System.Drawing.Point source)
        {
            var offset = GetOffsetPoint(new Point(source.X, source.Y));

            return new((int)offset.X, (int)offset.Y);
        }

        /// <summary>
        ///   Returns the source co-ordinates repositioned to include the current image offset and scaled by the current zoom level
        /// </summary>
        /// <param name="x">The source X co-ordinate.</param>
        /// <param name="y">The source Y co-ordinate.</param>
        /// <returns>A <see cref="Point"/> which has been repositioned to match the current zoom level and image offset</returns>
        public Point GetOffsetPoint(int x, int y)
        {
            return GetOffsetPoint(new System.Drawing.Point(x, y));
        }

        /// <summary>
        ///   Returns the source co-ordinates repositioned to include the current image offset and scaled by the current zoom level
        /// </summary>
        /// <param name="x">The source X co-ordinate.</param>
        /// <param name="y">The source Y co-ordinate.</param>
        /// <returns>A <see cref="Point"/> which has been repositioned to match the current zoom level and image offset</returns>
        public Point GetOffsetPoint(double x, double y)
        {
            return GetOffsetPoint(new Point(x, y));
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.PointF" /> repositioned to include the current image offset and scaled by the current zoom level
        /// </summary>
        /// <param name="source">The source <see cref="PointF"/> to offset.</param>
        /// <returns>A <see cref="PointF"/> which has been repositioned to match the current zoom level and image offset</returns>
        public Point GetOffsetPoint(Point source)
        {
            Rect viewport = GetImageViewPort();
            var scaled = GetScaledPoint(source);
            var offsetX = viewport.Left + Offset.X;
            var offsetY = viewport.Top + Offset.Y;

            return new(scaled.X + offsetX, scaled.Y + offsetY);
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.RectangleF" /> scaled according to the current zoom level and repositioned to include the current image offset
        /// </summary>
        /// <param name="source">The source <see cref="RectangleF"/> to offset.</param>
        /// <returns>A <see cref="RectangleF"/> which has been resized and repositioned to match the current zoom level and image offset</returns>
        public Rect GetOffsetRectangle(Rect source)
        {
            var viewport = GetImageViewPort();
            var scaled = GetScaledRectangle(source);
            var offsetX = viewport.Left - Offset.X;
            var offsetY = viewport.Top - Offset.Y;

            return new(new Point(scaled.Left + offsetX, scaled.Top + offsetY), scaled.Size);
        }

        /// <summary>
        ///   Returns the source rectangle scaled according to the current zoom level and repositioned to include the current image offset
        /// </summary>
        /// <param name="x">The X co-ordinate of the source rectangle.</param>
        /// <param name="y">The Y co-ordinate of the source rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <returns>A <see cref="Rectangle"/> which has been resized and repositioned to match the current zoom level and image offset</returns>
        public Rectangle GetOffsetRectangle(int x, int y, int width, int height)
        {
            return GetOffsetRectangle(new Rectangle(x, y, width, height));
        }

        /// <summary>
        ///   Returns the source rectangle scaled according to the current zoom level and repositioned to include the current image offset
        /// </summary>
        /// <param name="x">The X co-ordinate of the source rectangle.</param>
        /// <param name="y">The Y co-ordinate of the source rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <returns>A <see cref="RectangleF"/> which has been resized and repositioned to match the current zoom level and image offset</returns>
        public Rect GetOffsetRectangle(double x, double y, double width, double height)
        {
            return GetOffsetRectangle(new Rect(x, y, width, height));
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.Rectangle" /> scaled according to the current zoom level and repositioned to include the current image offset
        /// </summary>
        /// <param name="source">The source <see cref="Rectangle"/> to offset.</param>
        /// <returns>A <see cref="Rectangle"/> which has been resized and repositioned to match the current zoom level and image offset</returns>
        public Rectangle GetOffsetRectangle(Rectangle source)
        {
            var viewport = GetImageViewPort();
            var scaled = GetScaledRectangle(source);
            var offsetX = viewport.Left + Offset.X;
            var offsetY = viewport.Top + Offset.Y;

            return new(new System.Drawing.Point((int)(scaled.Left + offsetX), (int)(scaled.Top + offsetY)), new System.Drawing.Size((int)scaled.Size.Width, (int)scaled.Size.Height));
        }

        /// <summary>
        ///   Fits a given <see cref="T:System.Drawing.Rectangle" /> to match image boundaries
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <returns>
        ///   A <see cref="T:System.Drawing.Rectangle" /> structure remapped to fit the image boundaries
        /// </returns>
        public Rectangle FitRectangle(Rectangle rectangle)
        {
            var image = Image;
            if (image is null) return Rectangle.Empty;
            var x = rectangle.X;
            var y = rectangle.Y;
            var w = rectangle.Width;
            var h = rectangle.Height;

            if (x < 0)
            {
                x = 0;
            }

            if (y < 0)
            {
                y = 0;
            }

            if (x + w > image.Size.Width)
            {
                w = (int)(image.Size.Width - x);
            }

            if (y + h > image.Size.Height)
            {
                h = (int)(image.Size.Height - y);
            }

            return new(x, y, w, h);
        }

        /// <summary>
        ///   Fits a given <see cref="T:System.Drawing.RectangleF" /> to match image boundaries
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <returns>
        ///   A <see cref="T:System.Drawing.RectangleF" /> structure remapped to fit the image boundaries
        /// </returns>
        public Rect FitRectangle(Rect rectangle)
        {
            var image = Image;
            if (image is null) return Rect.Empty;
            var x = rectangle.X;
            var y = rectangle.Y;
            var w = rectangle.Width;
            var h = rectangle.Height;

            if (x < 0)
            {
                w -= -x;
                x = 0;
            }

            if (y < 0)
            {
                h -= -y;
                y = 0;
            }

            if (x + w > image.Size.Width)
            {
                w = image.Size.Width - x;
            }

            if (y + h > image.Size.Height)
            {
                h = image.Size.Height - y;
            }

            return new(x, y, w, h);
        }
        #endregion

        #region Navigate / Scroll methods
        /// <summary>
        ///   Scrolls the control to the given point in the image, offset at the specified display point
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to scroll to.</param>
        /// <param name="y">The Y co-ordinate of the point to scroll to.</param>
        /// <param name="relativeX">The X co-ordinate relative to the <c>x</c> parameter.</param>
        /// <param name="relativeY">The Y co-ordinate relative to the <c>y</c> parameter.</param>
        public void ScrollTo(double x, double y, double relativeX, double relativeY)
            => ScrollTo(new Point(x, y), new Point(relativeX, relativeY));

        /// <summary>
        ///   Scrolls the control to the given point in the image, offset at the specified display point
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to scroll to.</param>
        /// <param name="y">The Y co-ordinate of the point to scroll to.</param>
        /// <param name="relativeX">The X co-ordinate relative to the <c>x</c> parameter.</param>
        /// <param name="relativeY">The Y co-ordinate relative to the <c>y</c> parameter.</param>
        public void ScrollTo(int x, int y, int relativeX, int relativeY)
            => ScrollTo(new Point(x, y), new Point(relativeX, relativeY));

        /// <summary>
        ///   Scrolls the control to the given point in the image, offset at the specified display point
        /// </summary>
        /// <param name="imageLocation">The point of the image to attempt to scroll to.</param>
        /// <param name="relativeDisplayPoint">The relative display point to offset scrolling by.</param>
        public void ScrollTo(Point imageLocation, Point relativeDisplayPoint)
        {
            //CanRender = false;
            var zoomFactor = ZoomFactor;
            var x = imageLocation.X * zoomFactor - relativeDisplayPoint.X;
            var y = imageLocation.Y * zoomFactor - relativeDisplayPoint.Y;


            _canRender = true;
            Offset = new Vector(x, y);

            /*Debug.WriteLine(
                $"X/Y: {x},{y} | \n" +
                $"Offset: {Offset} | \n" +
                $"ZoomFactor: {ZoomFactor} | \n" +
                $"Image Location: {imageLocation}\n" +
                $"MAX: {HorizontalScrollBar.Maximum},{VerticalScrollBar.Maximum} \n" +
                $"ViewPort: {Viewport.Width},{Viewport.Height} \n" +
                $"Container: {HorizontalScrollBar.ViewportSize},{VerticalScrollBar.ViewportSize} \n" +
                $"Relative: {relativeDisplayPoint}");*/
        }

        /// <summary>
        ///   Centers the given point in the image in the center of the control
        /// </summary>
        /// <param name="imageLocation">The point of the image to attempt to center.</param>
        public void CenterAt(System.Drawing.Point imageLocation)
            => ScrollTo(new Point(imageLocation.X, imageLocation.Y), new Point(ViewPortSize.Width / 2, ViewPortSize.Height / 2));

        /// <summary>
        ///   Centers the given point in the image in the center of the control
        /// </summary>
        /// <param name="imageLocation">The point of the image to attempt to center.</param>
        public void CenterAt(Point imageLocation)
            => ScrollTo(imageLocation, new Point(ViewPortSize.Width / 2, ViewPortSize.Height / 2));

        /// <summary>
        ///   Centers the given point in the image in the center of the control
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to center.</param>
        /// <param name="y">The Y co-ordinate of the point to center.</param>
        public void CenterAt(int x, int y)
            => CenterAt(new Point(x, y));

        /// <summary>
        ///   Centers the given point in the image in the center of the control
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to center.</param>
        /// <param name="y">The Y co-ordinate of the point to center.</param>
        public void CenterAt(double x, double y)
            => CenterAt(new Point(x, y));

        /// <summary>
        /// Resets the viewport to show the center of the image.
        /// </summary>
        public void CenterToImage()
        {
            Offset = new Vector(HorizontalScrollBar.Maximum / 2, VerticalScrollBar.Maximum / 2);
        }
        #endregion

        #region Selection / ROI methods

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.Point" /> scaled according to the current zoom level
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to scale.</param>
        /// <param name="y">The Y co-ordinate of the point to scale.</param>
        /// <returns>A <see cref="Point"/> which has been scaled to match the current zoom level</returns>
        public Point GetScaledPoint(int x, int y)
        {
            return GetScaledPoint(new Point(x, y));
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.Point" /> scaled according to the current zoom level
        /// </summary>
        /// <param name="x">The X co-ordinate of the point to scale.</param>
        /// <param name="y">The Y co-ordinate of the point to scale.</param>
        /// <returns>A <see cref="Point"/> which has been scaled to match the current zoom level</returns>
        public PointF GetScaledPoint(float x, float y)
        {
            return GetScaledPoint(new PointF(x, y));
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.Point" /> scaled according to the current zoom level
        /// </summary>
        /// <param name="source">The source <see cref="Point"/> to scale.</param>
        /// <returns>A <see cref="Point"/> which has been scaled to match the current zoom level</returns>
        public Point GetScaledPoint(Point source)
        {
            return new(source.X * ZoomFactor, source.Y * ZoomFactor);
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.PointF" /> scaled according to the current zoom level
        /// </summary>
        /// <param name="source">The source <see cref="PointF"/> to scale.</param>
        /// <returns>A <see cref="PointF"/> which has been scaled to match the current zoom level</returns>
        public PointF GetScaledPoint(PointF source)
        {
            return new((float)(source.X * ZoomFactor), (float)(source.Y * ZoomFactor));
        }

        /// <summary>
        ///   Returns the source rectangle scaled according to the current zoom level
        /// </summary>
        /// <param name="x">The X co-ordinate of the source rectangle.</param>
        /// <param name="y">The Y co-ordinate of the source rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <returns>A <see cref="Rectangle"/> which has been scaled to match the current zoom level</returns>
        public Rect GetScaledRectangle(int x, int y, int width, int height)
        {
            return GetScaledRectangle(new Rect(x, y, width, height));
        }

        /// <summary>
        ///   Returns the source rectangle scaled according to the current zoom level
        /// </summary>
        /// <param name="x">The X co-ordinate of the source rectangle.</param>
        /// <param name="y">The Y co-ordinate of the source rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <returns>A <see cref="RectangleF"/> which has been scaled to match the current zoom level</returns>
        public RectangleF GetScaledRectangle(float x, float y, float width, float height)
        {
            return GetScaledRectangle(new RectangleF(x, y, width, height));
        }

        /// <summary>
        ///   Returns the source rectangle scaled according to the current zoom level
        /// </summary>
        /// <param name="location">The location of the source rectangle.</param>
        /// <param name="size">The size of the source rectangle.</param>
        /// <returns>A <see cref="Rectangle"/> which has been scaled to match the current zoom level</returns>
        public Rect GetScaledRectangle(Point location, Size size)
        {
            return GetScaledRectangle(new Rect(location, size));
        }

        /// <summary>
        ///   Returns the source rectangle scaled according to the current zoom level
        /// </summary>
        /// <param name="location">The location of the source rectangle.</param>
        /// <param name="size">The size of the source rectangle.</param>
        /// <returns>A <see cref="Rectangle"/> which has been scaled to match the current zoom level</returns>
        public RectangleF GetScaledRectangle(PointF location, SizeF size)
        {
            return GetScaledRectangle(new RectangleF(location, size));
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.Rectangle" /> scaled according to the current zoom level
        /// </summary>
        /// <param name="source">The source <see cref="Rectangle"/> to scale.</param>
        /// <returns>A <see cref="Rectangle"/> which has been scaled to match the current zoom level</returns>
        public Rect GetScaledRectangle(Rect source)
        {
            return new(source.Left * ZoomFactor, source.Top * ZoomFactor, source.Width * ZoomFactor, source.Height * ZoomFactor);
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.RectangleF" /> scaled according to the current zoom level
        /// </summary>
        /// <param name="source">The source <see cref="RectangleF"/> to scale.</param>
        /// <returns>A <see cref="RectangleF"/> which has been scaled to match the current zoom level</returns>
        public RectangleF GetScaledRectangle(RectangleF source)
        {
            return new((float)(source.Left * ZoomFactor), (float)(source.Top * ZoomFactor), (float)(source.Width * ZoomFactor), (float)(source.Height * ZoomFactor));
        }

        /// <summary>
        ///   Returns the source size scaled according to the current zoom level
        /// </summary>
        /// <param name="width">The width of the size to scale.</param>
        /// <param name="height">The height of the size to scale.</param>
        /// <returns>A <see cref="SizeF"/> which has been resized to match the current zoom level</returns>
        public SizeF GetScaledSize(float width, float height)
        {
            return GetScaledSize(new SizeF(width, height));
        }

        /// <summary>
        ///   Returns the source size scaled according to the current zoom level
        /// </summary>
        /// <param name="width">The width of the size to scale.</param>
        /// <param name="height">The height of the size to scale.</param>
        /// <returns>A <see cref="Size"/> which has been resized to match the current zoom level</returns>
        public Size GetScaledSize(int width, int height)
        {
            return GetScaledSize(new Size(width, height));
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.SizeF" /> scaled according to the current zoom level
        /// </summary>
        /// <param name="source">The source <see cref="SizeF"/> to scale.</param>
        /// <returns>A <see cref="SizeF"/> which has been resized to match the current zoom level</returns>
        public SizeF GetScaledSize(SizeF source)
        {
            return new((float)(source.Width * ZoomFactor), (float)(source.Height * ZoomFactor));
        }

        /// <summary>
        ///   Returns the source <see cref="T:System.Drawing.Size" /> scaled according to the current zoom level
        /// </summary>
        /// <param name="source">The source <see cref="Size"/> to scale.</param>
        /// <returns>A <see cref="Size"/> which has been resized to match the current zoom level</returns>
        public Size GetScaledSize(Size source)
        {
            return new(source.Width * ZoomFactor, source.Height * ZoomFactor);
        }

        /// <summary>
        ///   Creates a selection region which encompasses the entire image
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if no image is currently set</exception>
        public void SelectAll()
        {
            var image = Image;
            if (image is null) return;
            SelectionRegion = new Rect(0, 0, image.Size.Width, image.Size.Height);
        }

        /// <summary>
        /// Clears any existing selection region
        /// </summary>
        public void SelectNone()
        {
            SelectionRegion = Rect.Empty;
        }

        #endregion
        
        #region Viewport and image region methods
        /// <summary>
        ///   Gets the source image region.
        /// </summary>
        /// <returns></returns>
        public Rect GetSourceImageRegion()
        {
            var image = Image;
            if (image is null) return Rect.Empty;

            switch (SizeMode)
            {
                case SizeModes.Normal:
                    var offset = Offset;
                    var viewPort = GetImageViewPort();
                    var zoomFactor = ZoomFactor;
                    double sourceLeft = (offset.X / zoomFactor);
                    double sourceTop = (offset.Y / zoomFactor);
                    double sourceWidth = (viewPort.Width / zoomFactor);
                    double sourceHeight = (viewPort.Height / zoomFactor);

                    return new(sourceLeft, sourceTop, sourceWidth, sourceHeight);
            }

            return new(0, 0, image.Size.Width, image.Size.Height);

        }

        /// <summary>
        /// Gets the image view port.
        /// </summary>
        /// <returns></returns>
        public Rect GetImageViewPort()
        {
            if (ViewPortSize.Width == 0 && ViewPortSize.Height == 0) return Rect.Empty;

            double xOffset = 0;
            double yOffset = 0;
            double width = 0;
            double height = 0;

            switch (SizeMode)
            {
                case SizeModes.Normal:
                    if (AutoCenter)
                    {
                        xOffset = (!IsHorizontalBarVisible ? (ViewPortSize.Width - ScaledImageWidth) / 2 : 0);
                        yOffset = (!IsVerticalBarVisible ? (ViewPortSize.Height - ScaledImageHeight) / 2 : 0);
                    }

                    width = Math.Min(ScaledImageWidth - Math.Abs(Offset.X), ViewPortSize.Width);
                    height = Math.Min(ScaledImageHeight - Math.Abs(Offset.Y), ViewPortSize.Height);
                    break;
                case SizeModes.Stretch:
                    width = ViewPortSize.Width;
                    height = ViewPortSize.Height;
                    break;
                case SizeModes.Fit:
                    var image = Image;
                    double scaleFactor = Math.Min(ViewPortSize.Width / image.Size.Width, ViewPortSize.Height / image.Size.Height);
                    
                    width = Math.Floor(image.Size.Width * scaleFactor);
                    height = Math.Floor(image.Size.Height * scaleFactor);

                    if (AutoCenter)
                    {
                        xOffset = (ViewPortSize.Width - width) / 2;
                        yOffset = (ViewPortSize.Height - height) / 2;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(SizeMode), SizeMode, null);
            }

            return new(xOffset, yOffset, width, height);
        }
        #endregion

        #region Image methods
        public void LoadImage(string path)
        {
            Image = new Bitmap(path);
        }

        public Bitmap GetSelectedBitmap()
        {
            var image = ImageAsWriteableBitmap;
            if (image is null || !HaveSelection) return null;
            var selection = SelectionRegionNet;
            var pixelSize = SelectionPixelSize;
            using var frameBuffer = image.Lock();

            var newBitmap = new WriteableBitmap(pixelSize, image.Dpi, frameBuffer.Format, AlphaFormat.Unpremul);
            using var newFrameBuffer = newBitmap.Lock();

            int i = 0;

            unsafe
            {
                var inputPixels = (uint*) (void*) frameBuffer.Address;
                var targetPixels = (uint*) (void*) newFrameBuffer.Address;

                for (int y = selection.Y; y < selection.Bottom; y++)
                {
                    var thisY = y * frameBuffer.Size.Width;
                    for (int x = selection.X; x < selection.Right; x++)
                    {
                        targetPixels[i++] = inputPixels[thisY + x];
                    }
                }
            }

            return newBitmap;
        }
        #endregion

    }
}
