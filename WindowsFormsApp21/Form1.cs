using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp21
{
    public partial class Form1 : Form
    {

        public delegate void AsyncMove();
        Ball[] redball = new Ball[2];
        Ball[] blueball = new Ball[2];
        private CancellationTokenSource cancelToken = new CancellationTokenSource();
        public Form1()
        {
            for (int i = 0; i < 2; i++)
            {
                redball[i] = new Ball(new Point(0, ((i * 200) + 100)), "Right", true, Brushes.Red);
            }
            for (int i = 0; i < 2; i++)
            {
                blueball[i] = new Ball(new Point(1085, (i * 200) + 200), "Left", true, Brushes.Blue);
                redball[0].ChangeDirection += new AsyncMove(blueball[i].Nazad);
            }
            InitializeComponent();
            DoubleBuffered = true;
            for (int i = 0; i < 2; i++)
            {
                WaitCallback del = new WaitCallback(redball[i].RightMove); // инкапсуляция функции
                ThreadPool.QueueUserWorkItem(del); // запрос потока из пула потоков для выполнения функции
            }
                ParallelOptions partOpts = new ParallelOptions();
                partOpts.CancellationToken = cancelToken.Token;
                Task.Factory.StartNew(() =>
                {

                    Parallel.For(0, 2, cur =>
                    {
                        try
                        {
                            blueball[cur].LeftMove(partOpts);
                            Invoke((Action)delegate
                            {
                                Text = string.Format("Поток - {0}", cur);
                            });
                        }
                        catch (OperationCanceledException ex)
                        {
                            Invoke((Action)delegate
                            {
                                Text = string.Format("{0}", ex.Message);
                            });
                        }
                    });
                });

            }


        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            Invalidate();
        }
        private void стартToolStripMenuItem_Click(object sender, EventArgs timer1_Tick)
        {
            timer1.Start();
            
        }
        
        private void стопToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }
        public void Finish()
        {
            MessageBox.Show(
            "Финиш");
        }

        class Ball
        {
            public Point position;
            Brush color;
            Size size;
            bool stop;
            string direction;
            bool bluestart = false;

            public event AsyncMove ChangeDirection;

            public Ball(Point beginposition, string Direction, bool Stop, Brush Color)
            {
                this.color = Color;
                this.size = new Size(100, 100);
                this.position = beginposition;
                this.direction = Direction;
                this.stop = Stop;

            }
            public void draw(Graphics context)
            {
                context.FillEllipse(this.color, new Rectangle(this.position, this.size));
            }
            bool live = true;
            public void LeftMove(ParallelOptions obj)
            {

                while (live)
                {
                    try
                    {
                        obj.CancellationToken.ThrowIfCancellationRequested();
                        if (bluestart)
                            position.X -= 10;
                        Thread.Sleep(100);
                        if (position.X <= 30)
                        {
                            live = false;
                        }
                    }
                    catch(OperationCanceledException ex)
                    {
                        MessageBox.Show(string.Format("{0}", ex.Message));
                        live = false;
                    }
                }
            }
            public void Nazad()
            {
                bluestart = true;

            }

            public void RightMove(object obj)
            {

                while (direction == "Right" && stop == true)
                {
                    Thread.Sleep(100);
                    position.X += 10;
                    if (position.X >= 1090)
                    {
                        StopMove();
                        if (position.Y == 100)
                        {
                            Delegate[] delList = ChangeDirection.GetInvocationList(); // ассинхронное событие
                                                                                      // и выполнить их асинхронно
                            foreach (Delegate del in delList)
                            {
                                AsyncMove deleg = (AsyncMove)del; // Текущий делегат
                                deleg.BeginInvoke(null, null); // Выполнить
                            }
                        }
                    }
                }

            }

            public void StopMove()
            {
                this.stop = false;
            }


        }

        private void информацияОРазработчикеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Выполнил: студент группы 4307 Антаев М.П.\nВариант: №3", "Информация о разработчике");
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            redball[0].draw(e.Graphics);
            redball[1].draw(e.Graphics);
            blueball[0].draw(e.Graphics);
            blueball[1].draw(e.Graphics);
        }

        private void зановоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            cancelToken.Cancel();
        }
    }
}