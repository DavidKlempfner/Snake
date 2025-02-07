using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;

namespace Snake
{
    public class SnakeFood
    {
        private Window1 _snakeObject;
        public List<Point> RectList;

        public SnakeFood(Window1 snakeObject)//PASS IN _rects NOT WHOLE OBJECT
        {
            _snakeObject = snakeObject;
            BuildRectsArray();
        }

        public void BuildRectsArray()
        {
            RectList = new List<Point>();
            for (int i = 0; i < _snakeObject.NoOfColsAndRows; i++)
            {
                for (int j = 0; j < _snakeObject.NoOfColsAndRows; j++)
                {
                    if (_snakeObject.Rects[i, j] is Rectangle)
                    {
                        RectList.Add(new Point(i, j));
                    }
                }
            }
        }

        public void GenerateRandomNumber()
        {
            Random random = new Random();
            _snakeObject.CurrentFoodPt.X = random.Next(0, _snakeObject.NoOfColsAndRows);
            _snakeObject.CurrentFoodPt.Y = random.Next(0, _snakeObject.NoOfColsAndRows);

            while (RectList.Contains(_snakeObject.CurrentFoodPt))// doesn't work
            {
                _snakeObject.CurrentFoodPt.X = random.Next(0, _snakeObject.NoOfColsAndRows);
                _snakeObject.CurrentFoodPt.Y = random.Next(0, _snakeObject.NoOfColsAndRows);
            }
        }

        public void PlantFood(bool isInitialFood)
        {
            if (!isInitialFood)
            {
                GenerateRandomNumber();
            }
            Rectangle rect = new Rectangle();
            rect.Fill = Brushes.Blue;
            _snakeObject.SnakeGrid.Children.Add(rect);
            rect.SetValue(Grid.ColumnProperty, (int)_snakeObject.CurrentFoodPt.X);
            rect.SetValue(Grid.RowProperty, (int)_snakeObject.CurrentFoodPt.Y);
        }
    }
}
