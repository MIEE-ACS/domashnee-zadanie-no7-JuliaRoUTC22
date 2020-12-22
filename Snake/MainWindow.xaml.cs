using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Snake
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //поле, на котором живет змея
        Entity field;
        // голова змеи
        Head head;
        // вся змея
        List<PositionedEntity> snake;
        // яблоко
        Apple apple;
        // отравленное яблоко
        Poisoned_Apple_3 poisoned_apple_3;
        //количество очков
        int score;
        //для случайных чисел
        static Random rand = new Random();
        //таймер, по которому осуществляется движение
        DispatcherTimer moveTimer;
        // переменная, отвечающая за состояние здоровья змейки
        bool health = false;
        //таймер, по которому происходит снятие очков во время отравления
        DispatcherTimer poisoningTimer;


        //конструктор формы, выполняется при запуске программы
        public MainWindow()
        {
            InitializeComponent();
            
            snake = new List<PositionedEntity>();
            //создаем поле 300х300 пикселей
            field = new Entity(600, 600, "pack://application:,,,/Resources/field.png");

            //делаем текст с предупреждением об отравлении невидимым
            lbl_poison.Visibility = Visibility.Hidden;

            //создаем таймер, срабатывающий раз в 290 мс
            moveTimer = new DispatcherTimer();
            moveTimer.Interval = new TimeSpan(0, 0, 0, 0, 290);
            moveTimer.Tick += new EventHandler(moveTimer_Tick);

            //создаем таймер, срабатывающий раз в 1 с
            poisoningTimer = new DispatcherTimer();
            poisoningTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            poisoningTimer.Tick += new EventHandler(poisoningTimer_Tick);
 
        }

        //метод, перерисовывающий экран
        private void UpdateField()
        {
            //обновляем положение элементов змеи
            foreach (var p in snake)
            {
              Canvas.SetTop(p.image, p.y);
              Canvas.SetLeft(p.image, p.x);
            }

            //обновляем положение яблока
            Canvas.SetTop(apple.image, apple.y);
            Canvas.SetLeft(apple.image, apple.x);

            //обновляем положение отравленного яблока
             Canvas.SetTop(poisoned_apple_3.image, poisoned_apple_3.y);
             Canvas.SetLeft(poisoned_apple_3.image, poisoned_apple_3.x);
               
            //обновляем количество очков
            lblScore.Content = String.Format("{0}000", score);
        }

        //обработчик тика таймера. Всё движение происходит здесь.
        void moveTimer_Tick(object sender, EventArgs e)
        {
            //в обратном порядке двигаем все элементы змеи
            foreach (var p in Enumerable.Reverse(snake))
            {
                p.move();
            }

            //проверяем, что голова змеи не врезалась в тело
            foreach (var p in snake.Where(x => x != head))
            {
                //если координаты головы и какой либо из частей тела совпадают
                if (p.x == head.x && p.y == head.y)
                {
                    //мы проиграли
                    moveTimer.Stop();
                    poisoningTimer.Stop();

                    tbGameOver.Visibility = Visibility.Visible;
                    btn_start.Visibility = Visibility.Visible;
                    return;
                }
            }

            //проверяем, что голова змеи не вышла за пределы поля
            if (head.x < 40 || head.x >= 540 || head.y < 40 || head.y >= 540)
            {
                //мы проиграли
                moveTimer.Stop();
                poisoningTimer.Stop();

                tbGameOver.Visibility = Visibility.Visible;
                btn_start.Visibility = Visibility.Visible;
                return;
            }


            //проверяем, что голова змеи врезалась в отравленное яблоко
            if (head.x == poisoned_apple_3.x && head.y == poisoned_apple_3.y)
            {
                health = false;

                //делаем текст с предупреждением об отравлении
                lbl_poison.Visibility = Visibility.Visible;

                //съедаем отравленное яблоко
                canvas1.Children.Remove(poisoned_apple_3.image);

                //запускаем таймер
                poisoningTimer.Start();

                //двигаем отравленное яблоко на новое место
                poisoned_apple_3.move();
                
            }

          
            //проверяем, что голова змеи врезалась в яблоко
            if (head.x == apple.x && head.y == apple.y)
            {
                health = true;

                //убираем текст с предупреждением об отравлении
                lbl_poison.Visibility = Visibility.Hidden;

                //останавливаем таймер
                poisoningTimer.Stop();

                //увеличиваем счет
                score++;

                //убираем отравленное яблоко
                canvas1.Children.Remove(poisoned_apple_3.image);

                //двигаем яблоко на новое место
                apple.move();

                // добавляем новый сегмент к змее
                var part = new BodyPart(snake.Last());
                canvas1.Children.Add(part.image);
                snake.Add(part);

                // создаем отравленное яблоко с вероятностью 0,05
                if (rand.Next(1, 101) == 11 || rand.Next(1, 101) == 32 || rand.Next(1, 101) == 47 || rand.Next(1, 101) == 56 || rand.Next(1, 101) == 79) 
                {
                    health = false;
                    canvas1.Children.Add(poisoned_apple_3.image);

                    //двигаем отравленное яблоко на новое место
                    poisoned_apple_3.move();
                }
            }

            //перерисовываем экран
            UpdateField();
        }


        void poisoningTimer_Tick(object sender, EventArgs e)
        {
            if (!health)
            {
                //уменьшаем счет 
                score -= 1;
            }

            if (head.x == apple.x && head.y == apple.y)
            {
                poisoningTimer.Stop();
            
                canvas1.Children.Add(apple.image);
                apple.move();

                UpdateField();
            }
        }


        // Обработчик нажатия на кнопку клавиатуры
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    head.direction = Head.Direction.UP;
                    break;
                case Key.Down:
                    head.direction = Head.Direction.DOWN;
                    break;
                case Key.Left:
                    head.direction = Head.Direction.LEFT;
                    break;
                case Key.Right:
                    head.direction = Head.Direction.RIGHT;
                    break;
            }
        }

        // обработчик нажатия кнопки "Start"
        private void btn_start_Click(object sender, RoutedEventArgs e)
        {
            poisoningTimer.Stop();
            // обнуляем счёт
            score = 0;
            // обнуляем змею
            snake.Clear();
            // очищаем canvas
            canvas1.Children.Clear();
            // скрываем надпись "Game Over"
            tbGameOver.Visibility = Visibility.Hidden;
            // скрываем кнопку "start"
            btn_start.Visibility = Visibility.Hidden;

            // скрываем шкалу прогресса
            progress_bar.Visibility = Visibility.Hidden;
            lbl.Visibility = Visibility.Hidden;

            // скрываем текст-предупреждение об отравлении
            lbl_poison.Visibility = Visibility.Hidden;

            // добавляем поле на canvas
            canvas1.Children.Add(field.image);

            // создаем новое яблоко и добавляем его
            apple = new Apple(snake);
            canvas1.Children.Add(apple.image);
            
            // создаем новое отравленное яблоко и добавляем его
            poisoned_apple_3 = new Poisoned_Apple_3(snake);
            canvas1.Children.Add(poisoned_apple_3.image);

            // создаем голову
            head = new Head();
            snake.Add(head);
            canvas1.Children.Add(head.image);

            //запускаем таймер движения
            moveTimer.Start();
 
            UpdateField();

        }
        
        public class Entity
        {
            protected int m_width;
            protected int m_height;
            
            Image m_image;
            public Entity(int w, int h, string image)
            {
                m_width = w;
                m_height = h;
                m_image = new Image();
                m_image.Source = (new ImageSourceConverter()).ConvertFromString(image) as ImageSource;
                m_image.Width = w;
                m_image.Height = h;

            }

            public Image image
            {
                get
                {
                    return m_image;
                }
            }
        }

        public class PositionedEntity : Entity
        {
            protected int m_x;
            protected int m_y;
            public PositionedEntity(int x, int y, int w, int h, string image)
                : base(w, h, image)
            {
                m_x = x;
                m_y = y;
            }

            public virtual void move() { }

            public int x
            {
                get
                {
                    return m_x;
                }
                set
                {
                    m_x = value;
                }
            }

            public int y
            {
                get
                {
                    return m_y;
                }
                set
                {
                    m_y = value;
                }
            }
        }

        //создаём клас "яблоко"
        public class Apple : PositionedEntity
        {
            List<PositionedEntity> m_snake;
            public Apple(List<PositionedEntity> s)
                : base(0, 0, 40, 40, "pack://application:,,,/Resources/apple1.png")
            {
                m_snake = s;
                move();
            }

            public override void move()
            {
                Random rand = new Random();
                do
                {
                    x = rand.Next(13) * 40 + 40;
                    y = rand.Next(13) * 40 + 40;
                    bool overlap = false;
                    foreach (var p in m_snake)
                    {
                        if (p.x == x && p.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }
                    if (!overlap)
                        break;
                } while (true);

            }
        }

        //создаём класс "отравленное яблоко 3"
        public class Poisoned_Apple_3 : PositionedEntity
        {
            List<PositionedEntity> m_snake;
            public Poisoned_Apple_3(List<PositionedEntity> s)
                : base(0, 240, 40, 40, "pack://application:,,,/Resources/apple1.png")
            {
                m_snake = s;
                move();
            }

            public override void move()
            {
                Random rand = new Random();
                do
                {
                    x = rand.Next(12) * 40 + 40;
                    y = rand.Next(12) * 40 + 40;
                    bool overlap = false;
                    foreach (var p in m_snake)
                    {
                        if (p.x == x && p.y == y)
                        {
                            overlap = true;
                            break;
                        }
                    }
                    if (!overlap)
                        break;
                } while (true);

            }
        }

        public class Head : PositionedEntity
        {
            public enum Direction
            {
                RIGHT, DOWN, LEFT, UP, NONE
            };

            Direction m_direction;

            public Direction direction {
                set
                {
                    m_direction = value;
                    RotateTransform rotateTransform = new RotateTransform(90 * (int)value);
                    image.RenderTransform = rotateTransform;
                }
            }

            public Head()
                : base(280, 280, 40, 40, "pack://application:,,,/Resources/head_snake.png")
            {
                image.RenderTransformOrigin = new Point(0.5, 0.5);
                m_direction = Direction.NONE;
            }

            public override void move()
            {
                switch (m_direction)
                {
                    case Direction.DOWN:
                        y += 40;
                        break;
                    case Direction.UP:
                        y -= 40;
                        break;
                    case Direction.LEFT:
                        x -= 40;
                        break;
                    case Direction.RIGHT:
                        x += 40;
                        break;
                }
            }
        }

        public class BodyPart : PositionedEntity
        {
            PositionedEntity m_next;
            public BodyPart(PositionedEntity next)
                : base(next.x, next.y, 40, 40, "pack://application:,,,/Resources/body_snake.png")
            {
                m_next = next;
            }

            public override void move()
            {
                x = m_next.x;
                y = m_next.y;
            }
        }
    }
}
