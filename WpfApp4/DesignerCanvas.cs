using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;

namespace WpfApp4
{
    public class DesignerCanvas : Canvas
    {
        public DesignerCanvas()
        {
            this.AllowDrop = true;
            this.MouseLeftButtonDown += LelfButtonDown;
            this.MouseMove += Canvas_MouseMove;
            this.MouseLeftButtonUp += LeftButtonUp;
            List = new ObservableCollection<MyLine>();
            SelectedList = new ObservableCollection<MyLine>();
        }

        public FrameworkElement CurrentElement { get; set; }
        private MyLine CurrentCircle { get; set; }
        public Point BeforeMovingPosition { get; set; }
        public Point BeforeMovingCenter { get; set; }
        public ObservableCollection<MyLine> List { get; }
        public ObservableCollection<MyLine> SelectedList { get; }

        private void LelfButtonDown(object sender, MouseEventArgs e)
        {
            BeforeMovingPosition = e.GetPosition(this);
            var element = e.Source as FrameworkElement;
            CurrentElement = element;
            if (CurrentElement.Name == "Center")
            {
                var shape = CurrentElement.DataContext as MyLine;
                BeforeMovingCenter = shape.Center.Point;
                var prevx = shape.Start.Point - BeforeMovingCenter;
                var prevy = shape.End.Point - BeforeMovingCenter;
                var prez = prevx + prevy;

                shape.CenterRangeStart.Point = new Point(BeforeMovingCenter.X - prez.X, BeforeMovingCenter.Y - prez.Y);
                shape.CenterRangeEnd.Point = new Point(prez.X + BeforeMovingCenter.X, prez.Y + BeforeMovingCenter.Y);
            }
            if (CurrentElement.DataContext.ToString() == "WpfApp4.MyLine")
            {
                var shape = CurrentElement.DataContext as MyLine;
                shape.Center.Flag = true;
                shape.SetRadius = 5;
                CurrentCircle = shape;
            }
            if (CurrentElement.DataContext.ToString() != "WpfApp4.MyLine" && CurrentCircle != null)
            {
                CurrentCircle.Center.Flag = false;
                CurrentCircle.SetRadius = 0;
            }
            if (CurrentElement != null && CurrentElement.ToString() == "System.Windows.Shapes.Path")
            {
                var shape = CurrentElement.DataContext as MyLine;
                if (!List.Contains(shape))
                    List.Add(shape);

                shape.IsSelected = true;
                if (!SelectedList.Contains(shape))
                    SelectedList.Add(shape);
            }
            if (CurrentElement != null && CurrentElement.ToString() == "WpfApp4.DesignerCanvas" && SelectedList.Any())
            {
                foreach (var i in SelectedList)
                {
                    i.IsSelected = false;
                }
                SelectedList.Clear();
            }
            e.Handled = true;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && CurrentElement != null && CurrentElement.ToString() == "System.Windows.Shapes.Path")
            {
                var currentPoint = e.GetPosition(this);
                var shape = CurrentElement.DataContext as MyLine;

                var newX = currentPoint.X;
                var newY = currentPoint.Y;

                if (newX < 0) newX = 0;
                if (newX + CurrentElement.Width > this.ActualWidth) newX = this.ActualWidth - CurrentElement.Width;

                if (newY < 0) newY = 0;
                if (newY + CurrentElement.Height > this.ActualWidth) newY = this.ActualHeight - CurrentElement.Height;

                if (CurrentElement.Name == "StartPoint")
                {
                    shape.Start.Point = new Point(newX, newY);
                    DrawCircles(currentPoint, shape, "StartPoint");
                    //SnapConnection(shape, "StartPoint");
                }
                else if (CurrentElement.Name == "EndPoint")
                {
                    shape.End.Point = new Point(newX, newY);
                    DrawCircles(currentPoint, shape, "EndPoint");
                    //SnapConnection(shape, "EndPoint");
                }
                else if (CurrentElement.Name == "Center")
                {
                    var vX = shape.Start.Point - shape.Center.Point;
                    var StoE = shape.CenterRangeStart.Point - shape.CenterRangeStart.Point;
                    var StoP = currentPoint - shape.CenterRangeStart.Point;
                    var r = Vector.Multiply(StoE, StoP) / Math.Pow(StoE.Length, 2);
                    newX = shape.CenterRangeStart.Point.X + r * StoE.X;
                    newY = shape.CenterRangeStart.Point.Y + r * StoE.Y;

                    // 中心点の位置変更
                    shape.Center.Point = new Point(newX, newY);
                    var ZtoX = shape.Start.Point - shape.Center.Point;
                    var ZtoY = shape.End.Point - shape.Center.Point;
                    var diff = ZtoX.X - vX.X;// 移動前と後の差

                    shape.Radius = new Size(ZtoX.Length, ZtoY.Length);

                    // 変更後の中心点と半径が描く円周上に始点と終点を補正
                    DrawCircles(shape.Start.Point, shape, "StartPoint");
                    DrawCircles(shape.End.Point, shape, "EndPoint");
                    BeforeMovingCenter = shape.Center.Point;
                }
                else if (CurrentElement.Name == "Arc")
                {
                    ShiftCircles(newX, newY, shape);// 円弧の移動
                    BeforeMovingPosition = currentPoint;
                    SnapConnection();
                }
            }
        }

        private void LeftButtonUp(object sender, MouseEventArgs e)
        {
            CurrentElement = null;
        }

        private void DrawCircles(Point point, MyLine shape, string name)
        {
            var tan = Math.Atan2(point.Y - shape.Center.Point.Y, point.X - shape.Center.Point.X);// 動かした始点終点を画面左上を原点としたときのラジアン
            var cos = Math.Cos(tan);
            var sin = Math.Sin(tan);
            var x = shape.Center.Point.X + cos * shape.Radius.Width;// 原点を左上からCenterにするための数＋画面左上を原点としたときに描かれる円周上の始点または終点のx座標
            var y = shape.Center.Point.Y + sin * shape.Radius.Height;// 原点を左上からCenterにするための数＋画面左上を原点としたときに描かれる円周上の始点または終点のy座標

            var vX = shape.Start.Point - shape.Center.Point;
            var vY = shape.End.Point - shape.Center.Point;
            shape.Digree = Vector.AngleBetween(vX, vY);// ベクトル始点とベクトル終点のなす角

            if (name == "StartPoint")
            {
                shape.Start.Point = new Point(x, y);
            }
            else if (name == "EndPoint")
            {
                shape.End.Point = new Point(x, y);
            }
        }

        private void ShiftCircles(double newX, double newY, MyLine shape)
        {
            // 移動前との差異
            var diffX = newX - BeforeMovingPosition.X;
            var diffY = newY - BeforeMovingPosition.Y;
            // 始点,終点,中心点を差異だけ加算
            var v = new Vector(diffX, diffY);
            foreach (var i in SelectedList)
            {
                i.Start.Point += v;
                i.End.Point += v;
                i.Center.Point += v;
            }
        }

        private void SnapConnection()
        {
            if (SelectedList.Any() && SelectedList.Count == 1)
            {
                var movingShape = SelectedList[0];
                var AccessLineList = List.Where(v => v != movingShape);
                if (CurrentElement.Name == "StartPoint" || CurrentElement.Name == "Arc")
                {
                    foreach(var AccessLine in AccessLineList)
                    {
                        var StoS = AccessLine.Start.Point - movingShape.Start.Point;
                        var StoE = AccessLine.End.Point - movingShape.Start.Point;
                        if (StoS.Length < 10)
                        {
                            movingShape.Start.Point = AccessLine.Start.Point;
                            if (CurrentElement.Name == "Arc")
                            {
                                movingShape.End.Point += StoS;
                                movingShape.Center.Point += StoS;
                            }

                            if (!movingShape.Start.AccessLine.Contains(AccessLine))
                                movingShape.Start.AccessLine.Add(AccessLine);
                            if (!AccessLine.Start.AccessLine.Contains(movingShape))
                                AccessLine.Start.AccessLine.Add(movingShape);
                        }

                        if (StoE.Length < 10)
                        {
                            movingShape.Start.Point = AccessLine.End.Point;
                            if (CurrentElement.Name == "Arc")
                            {
                                movingShape.End.Point += StoE;
                                movingShape.Center.Point += StoE;
                            }

                            if (!movingShape.Start.AccessLine.Contains(AccessLine))
                                movingShape.Start.AccessLine.Add(AccessLine);
                            if (!AccessLine.End.AccessLine.Contains(movingShape))
                                AccessLine.End.AccessLine.Add(movingShape);
                        }
                    }
                }
                if (CurrentElement.Name == "EndPoint" || CurrentElement.Name == "Arc")
                {
                    foreach (var AccessLine in AccessLineList)
                    {
                        var EtoS = AccessLine.Start.Point - movingShape.End.Point;
                        var EtoE = AccessLine.End.Point - movingShape.End.Point;
                        if (EtoS.Length < 10)
                        {
                            movingShape.End.Point = AccessLine.Start.Point;
                            if (CurrentElement.Name == "Arc")
                            {
                                movingShape.Start.Point += EtoS;
                                movingShape.Center.Point += EtoS;
                            }

                            if (!movingShape.End.AccessLine.Contains(AccessLine))
                                movingShape.End.AccessLine.Add(AccessLine);
                            if (!AccessLine.Start.AccessLine.Contains(movingShape))
                                AccessLine.Start.AccessLine.Add(movingShape);
                        }

                        if (EtoE.Length < 10)
                        {
                            movingShape.End.Point = AccessLine.End.Point;
                            if (CurrentElement.Name == "Arc")
                            {
                                movingShape.Start.Point += EtoE;
                                movingShape.Center.Point += EtoE;
                            }

                            if (!movingShape.End.AccessLine.Contains(AccessLine))
                                movingShape.End.AccessLine.Add(AccessLine);
                            if (!AccessLine.End.AccessLine.Contains(movingShape))
                                AccessLine.End.AccessLine.Add(movingShape);
                        }
                    }
                }

                CheckConnectionStatus(AccessLineList, movingShape);
            }
            else if (SelectedList.Any() && SelectedList.Count > 1)
            {
                foreach(var movingShape in SelectedList)
                {
                    var AccessLineList = List.Where(v => v != movingShape).Where(v => v.IsSelected == false);
                    var ConnectedList = SelectedList.Where(v => v != movingShape);
                    if (CurrentElement.Name == "StartPoint" || CurrentElement.Name == "Arc")
                    {
                        foreach (var AccessLine in AccessLineList)
                        {
                            var StoS = AccessLine.Start.Point - movingShape.Start.Point;
                            var StoE = AccessLine.End.Point - movingShape.Start.Point;
                            if (StoS.Length < 10)
                            {
                                movingShape.Start.Point = AccessLine.Start.Point;
                                if (CurrentElement.Name == "Arc")
                                {
                                    movingShape.End.Point += StoS;
                                    movingShape.Center.Point += StoS;
                                }
                                if (CurrentElement.Name != "StartPoint")
                                {
                                    foreach (var connectedLine in ConnectedList)
                                    {
                                        connectedLine.Start.Point += StoS;
                                        connectedLine.End.Point += StoS;
                                        connectedLine.Center.Point += StoS;
                                    }
                                }
                                if (!movingShape.Start.AccessLine.Contains(AccessLine))
                                    movingShape.Start.AccessLine.Add(AccessLine);
                                if (!AccessLine.Start.AccessLine.Contains(movingShape))
                                    AccessLine.Start.AccessLine.Add(movingShape);
                            }
                            if (StoE.Length < 10)
                            {
                                movingShape.Start.Point = AccessLine.End.Point;
                                if (CurrentElement.Name == "Arc")
                                {
                                    movingShape.End.Point += StoE;
                                    movingShape.Center.Point += StoE;
                                }
                                if (CurrentElement.Name != "StartPoint")
                                {
                                    foreach (var connectedLine in ConnectedList)
                                    {
                                        connectedLine.Start.Point += StoE;
                                        connectedLine.End.Point += StoE;
                                        connectedLine.Center.Point += StoE;
                                    }
                                }
                                if (!movingShape.Start.AccessLine.Contains(AccessLine))
                                    movingShape.Start.AccessLine.Add(AccessLine);
                                if (!AccessLine.End.AccessLine.Contains(movingShape))
                                    AccessLine.End.AccessLine.Add(movingShape);
                            }
                        }
                    }
                    if (CurrentElement.Name == "EndPoint" || CurrentElement.Name == "Arc")
                    {
                        foreach (var AccessLine in AccessLineList)
                        {
                            var EtoS = AccessLine.Start.Point - movingShape.End.Point;
                            var EtoE = AccessLine.End.Point - movingShape.End.Point;
                            if (EtoS.Length < 10)
                            {
                                movingShape.End.Point = AccessLine.Start.Point;
                                if (CurrentElement.Name == "Arc")
                                {
                                    movingShape.Start.Point += EtoS;
                                    movingShape.Center.Point += EtoS;
                                }
                                if (CurrentElement.Name != "EndPoint")
                                {
                                    foreach (var connectedLine in ConnectedList)
                                    {
                                        connectedLine.Start.Point += EtoS;
                                        connectedLine.End.Point += EtoS;
                                        connectedLine.Center.Point += EtoS;
                                    }
                                }
                                if (!movingShape.End.AccessLine.Contains(AccessLine))
                                    movingShape.End.AccessLine.Add(AccessLine);
                                if (!AccessLine.Start.AccessLine.Contains(movingShape))
                                    AccessLine.Start.AccessLine.Add(movingShape);
                            }
                            if (EtoE.Length < 10)
                            {
                                movingShape.End.Point = AccessLine.End.Point;
                                if (CurrentElement.Name == "Arc")
                                {
                                    movingShape.Start.Point += EtoE;
                                    movingShape.Start.Point += EtoE;
                                }
                                if (CurrentElement.Name != "EndPoint")
                                {
                                    foreach (var connectedLine in ConnectedList)
                                    {
                                        connectedLine.Start.Point += EtoE;
                                        connectedLine.End.Point += EtoE;
                                        connectedLine.Center.Point += EtoE;
                                    }
                                }
                                if (!movingShape.End.AccessLine.Contains(AccessLine))
                                    movingShape.End.AccessLine.Add(AccessLine);
                                if (!AccessLine.End.AccessLine.Contains(movingShape))
                                    AccessLine.End.AccessLine.Add(movingShape);
                            }
                        }
                    }

                    CheckConnectionStatus(AccessLineList, movingShape);
                }
            }
        }

        private void CheckConnectionStatus(IEnumerable<MyLine> AccsessLineList, MyLine movingShape)
        {
            foreach (var AccessLine in AccsessLineList)
            {
                if (movingShape.Start.AccessLine.Contains(AccessLine) || movingShape.Start.AccessLine.Contains(AccessLine))
                {
                    if (AccessLine.Start.AccessLine.Contains(movingShape))
                    {
                        if (movingShape.Start.Point != AccessLine.Start.Point)
                        {
                            movingShape.Start.AccessLine.Remove(AccessLine);
                            AccessLine.Start.AccessLine.Remove(movingShape);
                        }
                    }
                    if (AccessLine.End.AccessLine.Contains(movingShape))
                    {
                        if (movingShape.Start.Point != AccessLine.End.Point)
                        {
                            movingShape.Start.AccessLine.Remove(AccessLine);
                            AccessLine.End.AccessLine.Remove(movingShape);
                        }
                    }
                }

                if (movingShape.End.AccessLine.Contains(AccessLine) || movingShape.End.AccessLine.Contains(AccessLine))
                {
                    if (AccessLine.Start.AccessLine.Contains(movingShape))
                    {
                        if (movingShape.End.Point != AccessLine.Start.Point)
                        {
                            movingShape.End.AccessLine.Remove(AccessLine);
                            AccessLine.Start.AccessLine.Remove(movingShape);
                        }
                    }
                    if (AccessLine.End.AccessLine.Contains(movingShape))
                    {
                        if (movingShape.End.Point != AccessLine.End.Point)
                        {
                            movingShape.End.AccessLine.Remove(AccessLine);
                            AccessLine.End.AccessLine.Remove(movingShape);
                        }
                    }
                }
            }
        }
    }
}
