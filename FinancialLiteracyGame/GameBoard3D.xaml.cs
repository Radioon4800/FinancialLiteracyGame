using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FinancialLiteracyGame
{
    public partial class GameBoard3D : UserControl
    {
        private ModelVisual3D playerFigure;
        private int currentCellIndex = 0;

        public GameBoard3D()
        {
            InitializeComponent();
            CreateCylinderField(24);
            // Создаем фигуру сразу после поля
            CreatePlayerFigure();
        }

        public void RotateBoard(double deltaAngle)
        {
            if (BoardRotation != null)
            {
                BoardRotation.Angle += deltaAngle;
            }
        }

        public void CreateCylinderField(int cellCount = 24)
        {
            Model3DGroup mainGroup = new Model3DGroup();
            double radius = 6;
            double innerRadius = 4;
            double height = 0.5;

            for (int i = 0; i < cellCount; i++)
            {
                double angleStart = (double)i / cellCount * 2 * Math.PI;
                double angleEnd = (double)(i + 1) / cellCount * 2 * Math.PI;

                MeshGeometry3D mesh = new MeshGeometry3D();

                Point3D p1 = new Point3D(Math.Cos(angleStart) * radius, height, Math.Sin(angleStart) * radius);
                Point3D p2 = new Point3D(Math.Cos(angleEnd) * radius, height, Math.Sin(angleEnd) * radius);
                Point3D p3 = new Point3D(Math.Cos(angleEnd) * innerRadius, height, Math.Sin(angleEnd) * innerRadius);
                Point3D p4 = new Point3D(Math.Cos(angleStart) * innerRadius, height, Math.Sin(angleStart) * innerRadius);

                mesh.Positions.Add(p1); mesh.Positions.Add(p2);
                mesh.Positions.Add(p3); mesh.Positions.Add(p4);

                mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(1); mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add(2); mesh.TriangleIndices.Add(3);

                var color = (i % 2 == 0) ? Brushes.RoyalBlue : Brushes.WhiteSmoke;
                if (i % 6 == 0) color = Brushes.Gold;

                var material = new DiffuseMaterial(color);
                var model = new GeometryModel3D(mesh, material) { BackMaterial = material };
                mainGroup.Children.Add(model);
            }

            // ПРИСВАИВАНИЕ ДОЛЖНО БЫТЬ ЗДЕСЬ
            CylinderBoardModel.Content = mainGroup;
        }

        public void CreatePlayerFigure()
        {
            if (playerFigure != null) CylinderBoardModel.Children.Remove(playerFigure);

            playerFigure = new ModelVisual3D();
            MeshGeometry3D mesh = new MeshGeometry3D();

            double r = 0.4;    // Радиус фишки
            double h = 0.2;    // Высота (шайба)
            int sides = 18;    // На сколько граней разбит круг

            // Генерируем вершины цилиндра
            for (int i = 0; i < sides; i++)
            {
                double theta = 2 * Math.PI * i / sides;
                double x = Math.Cos(theta) * r;
                double z = Math.Sin(theta) * r;

                mesh.Positions.Add(new Point3D(x, h, z)); // Верх (индексы 0, 2, 4...)
                mesh.Positions.Add(new Point3D(x, 0, z)); // Низ  (индексы 1, 3, 5...)
            }

            // Рисуем боковые грани и крышку
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                // Бока
                mesh.TriangleIndices.Add(i * 2); mesh.TriangleIndices.Add(next * 2); mesh.TriangleIndices.Add(i * 2 + 1);
                mesh.TriangleIndices.Add(next * 2); mesh.TriangleIndices.Add(next * 2 + 1); mesh.TriangleIndices.Add(i * 2 + 1);

                // Простая плоская крышка (заполняем треугольниками от первой вершины)
                if (i > 0 && i < sides - 1)
                {
                    mesh.TriangleIndices.Add(0); mesh.TriangleIndices.Add((i + 1) * 2); mesh.TriangleIndices.Add(i * 2);
                }
            }

            // Красивый золотой материал с небольшим блеском
            MaterialGroup goldMaterial = new MaterialGroup();
            goldMaterial.Children.Add(new DiffuseMaterial(Brushes.Gold));
            goldMaterial.Children.Add(new SpecularMaterial(Brushes.White, 40));

            playerFigure.Content = new GeometryModel3D(mesh, goldMaterial);

            // Добавляем фишку как ребенка поля, чтобы она крутилась вместе с ним
            CylinderBoardModel.Children.Add(playerFigure);

            MovePlayer(0);
        }

        public void MovePlayer(int cellIndex)
        {
            if (playerFigure == null) return;

            currentCellIndex = cellIndex % 24;
            double fieldRadius = 5.0;
            double angle = (currentCellIndex + 0.5) * (2 * Math.PI / 24);

            double x = Math.Cos(angle) * fieldRadius;
            double z = Math.Sin(angle) * fieldRadius;

            // Устанавливаем высоту 0.5 (верхняя граница поля)
            // Назначаем трансформацию напрямую, без лишних переменных
            playerFigure.Transform = new TranslateTransform3D(x, 0.5, z);
        }
    }
}