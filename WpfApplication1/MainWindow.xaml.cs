using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using System.Drawing;
using System.Diagnostics;

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        long startTime;
        long prevTime;

        int i=0,j=0;
        //Image[] pic = new Image[15]; // create two (empty) global pixmaps
        Image bg = new Image();
        Image gg = new Image();
        Image[] bu = new Image[8];
        Image[] boss = new Image[5];
        Image[] wave = new Image[5];
        Image[] bomb = new Image[2];
        Image[] pic = new Image[15];
        int whichPic = 0;
        int whichBoss = 0; // which pixmap to display
        int picX=100, picY=120;
        int[] buX = new int[8], buY = new int[8];
        int[] waveX = new int[5],waveY = new int[5];
        int bossX=600, bossY=120;
        int[] bombX = new int[2],bombY = new int[2];
        int bossLife=40;
        int dist;
        int[] fired= new int[8];
        bool[] waved = new bool[5],bombed = new bool[2];
        bool movingL=false,movingR=false,dead=false,vani=false,started=false;
        int gameState=0;
        long totalTime;
        int jumpState=0, dashState=0;
        int[] buDir = new int[8];
        int picDir=0,bossDir=1;
        MediaPlayer bgm = new MediaPlayer(); 

        public MainWindow()
        {
            InitializeComponent();

            InitImages();
            RenderOptions.SetBitmapScalingMode(canvas, BitmapScalingMode.NearestNeighbor);

            bgm.Open(new Uri("../../sound/bgm.mp3", UriKind.Relative));
            bgm.MediaEnded += new EventHandler(Media_Ended);
            bgm.Play();

            CompositionTarget.Rendering += Loop;
        }

        void InitImages() {
            ImageSource src = new BitmapImage(new Uri("../../image/background.png", UriKind.Relative));
            bg = new Image { Width = src.Width, Height = src.Height, Source = src };
            /*src = new BitmapImage(new Uri("../../image/white.png", UriKind.Relative));
            gg = new Image { Width = src.Width, Height = src.Height, Source = src };
            src = new BitmapImage(new Uri("../../image/megaman0.png", UriKind.Relative));
            pic[0] = new Image { Width = src.Width, Height = src.Height, Source = src };*/
            src = new BitmapImage(new Uri("../../image/bullet.png", UriKind.Relative));
            for (int i = 0; i < bu.Length-1; i++ )
                bu[i] = new Image { Width = src.Width, Height = src.Height, Source = src };
            src = new BitmapImage(new Uri("../../image/wave0.png", UriKind.Relative));
            bu[7] = new Image { Width = src.Width, Height = src.Height, Source = src };
            src = new BitmapImage(new Uri("../../image/wave1.png", UriKind.Relative));
            for (int i = 0; i < wave.Length; i++)
                wave[i] = new Image { Width = src.Width, Height = src.Height, Source = src };
            for (int i = 0; i < bomb.Length; i++)
            {
                src = new BitmapImage(new Uri("../../image/bomb" + i + ".png", UriKind.Relative));
                bomb[i] = new Image { Width = src.Width, Height = src.Height, Source = src };
            }
            for (int i = 0; i < pic.Length; i++)
            {
                src = new BitmapImage(new Uri("../../image/megaman" + i + ".png", UriKind.Relative));
                pic[i] = new Image { Width = src.Width, Height = src.Height, Source = src };
            }
            for (int i = 0; i < boss.Length; i++)
            {
                src = new BitmapImage(new Uri("../../image/iceman" + i + ".png", UriKind.Relative));
                boss[i] = new Image { Width = src.Width, Height = src.Height, Source = src };
            }
        }

        void Loop(object obj, EventArgs e)
        {
            prevTime = DateTime.Now.Second;


            canvas.Children.Clear();

            Draw(bg, 0, 0, 3, 3);

            if (picX < 0)
                picX = 0;
            if (picX + pic[whichPic].Width*2.5 > 800)
            {
                picX = (int)(800 - pic[whichPic].Width*2.5);
            }

            Draw(pic[whichPic], picX, picY, picDir == 0 ? 2.5 : -2.5, 2.5);

            for (int i = 0; i < 7; i++)
            {
                if (fired[i] == 1)
                    Draw(bu[i], buX[i], buY[i], 2.5, 2.5);
            }

            if (fired[7] != 0)
            {
                if (buDir[7] == 0)
                    Draw(bu[7], buX[7], buY[7], -4, 4);
                else
                    Draw(bu[7], buX[7], buY[7], 4, 4);
            }

            for (int i = 0; i < 5; i++)
            {
                if (waved[i] && bossLife > 0)
                    Draw(wave[i], waveX[i], waveY[i], 5, 5);
            }

            for (int i = 0; i < 2; i++)
            {
                if (bombed[i])
                    Draw(bomb[i], bombX[i], bombY[i], 1.5, 1.5);
            }

            if (!vani && bossLife > 0)
            {
                if (bossDir == 1)
                {
                    Draw(boss[whichBoss], bossX, bossY, 3, 3);
                }
                else
                {
                    Draw(boss[whichBoss], bossX, bossY, -3, 3);
                }
            }

            bool touch = !AABBtest(picX, picY, picX + (float)pic[whichPic].Width * 2.5f, picY + (float)pic[whichPic].Height * 2.5f, bossX, bossY, bossX + (float)boss[whichBoss].Width * 3, bossY + (float)boss[whichBoss].Height * 3);

            bool hit = (!AABBtest(buX[7], buY[7], buX[7] + (float)bu[7].Width * 4, buY[7] + (float)bu[7].Height * 4, picX, picY, picX + (float)pic[whichPic].Width * 2.5f, picY + (float)pic[whichPic].Height * 2.5f));

            for (int i = 0; i < 5; i++)
                if (!AABBtest(waveX[i] + 5, waveY[i], waveX[i] + (float)wave[i].Width * 5 - 5, waveY[i] + (float)wave[i].Height * 5, picX, picY, picX + (float)pic[whichPic].Width * 2.5f, picY + (float)pic[whichPic].Height * 2.5f))
                    hit = true;

            if ((touch || hit) && !dead && bossLife > 0)
            {
                dead = true;
                picY += 1;
                if (picX < bossX)
                    picDir = 0;
                else
                    picDir = 1;
                lose(0);
            }

            for (int i = 0; i < 7; i++)
            {
                bool shot = (!AABBtest(buX[i], buY[i], buX[i] + (float)bu[i].Width, buY[i] + (float)bu[i].Height, bossX, bossY, bossX + (float)boss[whichBoss].Width * 3, bossY + (float)boss[whichBoss].Height * 3) && bossLife > 0 && !dead);
                if (shot)
                {
                    fired[i] = 2;
                    buX[i] = 0;
                    buY[i] = 0;
                    bossLife--;
                    blink(0);
                }
            }

            if (gameState == 0)
            {
                char[] str = new char[41];
                for (int i = 0; i < 41; i++)
                {
                    if (i < bossLife)
                        str[i] = '|';
                    else
                    {
                        str[i] = ' ';
                        //break;
                    }
                }
                Draw("BOSS " + new String(str), 300, 550, Color.FromRgb(0, 0, 230), 18);
            }
            else
            {
                if (gameState == 1)
                {
                    Draw("GAME OVER...", 320, 400, Color.FromRgb(0, 0, 0), 18);
                    Draw("press R to restart, Q to quit", 280, 360, Color.FromRgb(0, 0, 0), 18);
                }
                else if (gameState == 2)
                {
                    Draw("VICTORY!!!", 320, 400, Color.FromRgb(0, 0, 0), 18);
                    Draw("Time: " + (int)totalTime + " second", 300, 360, Color.FromRgb(0, 0, 0), 18);
                    Draw("press R to restart, Q to quit", 280, 320, Color.FromRgb(0, 0, 0), 18);
                }
            }

            if (!started)
            {
                Draw("LEFT and RIGHT to move", 240, 460, Color.FromRgb(0, 0, 0), 18);
                Draw("Z to dash, X to jump, C or SPACE to shoot", 240, 420, Color.FromRgb(0, 0, 0), 18);
                Draw("press S to start", 240, 380, Color.FromRgb(0, 0, 0), 18);
            }
        }

        public void Draw(Image img, int posX, int posY, double scaleX, double scaleY)
        {
            img.RenderTransform = new ScaleTransform(scaleX, scaleY);
            if (scaleX < 0) posX += (int)(-scaleX * img.Width);
            canvas.Children.Add(img);
            Canvas.SetLeft(img, posX);
            Canvas.SetTop(img, 600-img.Height * scaleY - posY);
        }

        public void Draw(String text, int posX, int posY , Color color, double fontSize)
        {
            TextBlock tb = new TextBlock();
            tb.Text = text;
            tb.Foreground = new SolidColorBrush(color);
            tb.FontSize = fontSize;
            canvas.Children.Add(tb);
            Canvas.SetLeft(tb, posX);
            Canvas.SetTop(tb, 600 - posY);
        }

        bool AABBtest(float ax1, float ay1, float ax2, float ay2, float bx1, float by1, float bx2, float by2)
        {
            return
                ax1 > bx2 || ax2 < bx1 ||
                ay1 > by2 || ay2 < by1;
        }

        async void moveLeft(int i)
        {
            if (dead) return;
            if (movingL)
            {
                picX -= 7;
                picDir = 1;

                if (i % 4 == 0 && dashState == 0)
                {
                    if (whichPic == 0 || whichPic == 6) whichPic = 1;
                    else if (whichPic == 1) whichPic = 2;
                    else if (whichPic == 2) whichPic = 3;
                    else if (whichPic == 3) whichPic = 8;
                    else if (whichPic == 8) whichPic = 1;

                    if (fired[0] == 1 || fired[1] == 1 || fired[2] == 1 || fired[3] == 1 || fired[4] == 1 || fired[5] == 1 || fired[6] == 1)
                    {
                        if (whichPic == 9) whichPic = 10;
                        else if (whichPic == 10) whichPic = 11;
                        else if (whichPic == 11) whichPic = 12;
                        else if (whichPic == 12) whichPic = 9;
                        else if (whichPic == 4 || whichPic == 7) whichPic = 7;
                        else whichPic = 12;
                    }
                }

                i++;
                await Task.Delay(20);
                moveLeft(i);
            }
            else
            {
                if (jumpState==0)
                {
                    if (fired[0] == 1 || fired[1] == 1 || fired[2] == 1 || fired[3] == 1 || fired[4] == 1 || fired[5] == 1 || fired[6] == 1)
                        whichPic = 6;
                    else
                        whichPic = 0;
                }
                //movingL=0;
            }
        }

        async void moveRight(int i)
        {
            if (dead) return;
            if (movingR)
            {
                picX += 7;
                picDir = 0;

                if (i % 4 == 0 && dashState==0)
                {
                    if (whichPic == 0 || whichPic == 6) whichPic = 1;
                    else if (whichPic == 1) whichPic = 2;
                    else if (whichPic == 2) whichPic = 3;
                    else if (whichPic == 3) whichPic = 8;
                    else if (whichPic == 8) whichPic = 1;

                    if (fired[0] == 1 || fired[1] == 1 || fired[2] == 1 || fired[3] == 1 || fired[4] == 1 || fired[5] == 1 || fired[6] == 1)
                    {
                        if (whichPic == 9) whichPic = 10;
                        else if (whichPic == 10) whichPic = 11;
                        else if (whichPic == 11) whichPic = 12;
                        else if (whichPic == 12) whichPic = 9;
                        else if (whichPic == 4 || whichPic == 7) whichPic = 7;
                        else whichPic = 12;
                    }
                }

                i++;
                await Task.Delay(20);
                moveRight(i);
            }
            else
            {
                if (jumpState == 0)
                {
                    if (fired[0] == 1 || fired[1] == 1 || fired[2] == 1 || fired[3] == 1 || fired[4] == 1 || fired[5] == 1 || fired[6] == 1)
                        whichPic = 6;
                    else
                        whichPic = 0;
                }
                //movingR=0;
            }
        }

        async void jump(int i)
        {
            if (!started)
                return;

            if (whichPic != 7 && whichPic != 5) whichPic = 4;

            if (i < 35 && !dead)
            {
                if (i < 18)
                {
                    picY += (17 - i);
                }
                else
                {
                    picY -= (i - 17);
                }
                i++;
                await Task.Delay(20);
                jump(i);
            }
            else
            {
                if (fired[0] == 1 || fired[1] == 1 || fired[2] == 1 || fired[3] == 1 || fired[4] == 1 || fired[5] == 1 || fired[6] == 1)
                    whichPic = 6;
                else
                    whichPic = 0;
                jumpState = 0;
            }
        }

        async void dash(int i)
        {
            if (!started)
                return;

            whichPic = 5;
            if (i < 15 && !dead)
            {
                if (picDir == 0)
                {
                    if (movingR)
                        picX += 5;
                    else
                        picX += 12;
                }
                else
                {
                    if (movingL)
                        picX -= 5;
                    else
                        picX -= 12;
                }
                i++;
                await Task.Delay(20);
                dash(i);
            }
            else
            {
                whichPic = 0;
                dashState = 0;
            }
        }

        async void fire(int i)
        {
            if (!started)
                return;
            if (buX[i] >= -20 && buX[i] < 810 && fired[i] == 1)
            {
                if (buDir[i] == 0)
                    buX[i] += 20;
                else
                    buX[i] -= 20;
                await Task.Delay(20);
                fire(i);
            }
            else
            {
                if (i != 7)
                    fired[i] = 2;
                else
                    fired[i] = 0;
                buX[i] = 0;
                buY[i] = 0;
                if (i <= 6)
                {
                    if (picY == 120)
                    {
                        if (fired[0] != 1 && fired[1] != 1 && fired[2] != 1 && fired[3] != 1 && fired[4] != 1 && fired[5] != 1 && fired[6] != 1)
                            if (!movingL && !movingR)
                                whichPic = 0;
                            else if (whichPic != 5)
                                whichPic = 1;
                    }
                }
                else
                {
                    if (bossY == 120)
                    {
                        if (fired[7] == 0)
                            whichBoss = 0;
                    }
                }
            }
        }

        async void blink(int i)
        {
            if (i < 2)
            {
                if (i == 1)
                    vani = true;
                i++;
                await Task.Delay(30);
                blink(i);
            }
            else
            {
                vani = false;
            }
        }

        async void win(int i)
        {
            if (!started)
                return;

            vani = true;
            if (i < 10)
            {
                if (i % 2 == 0)
                {
                    bombed[0] = true;
                    bombed[1] = false;
                }
                else
                {
                    bombed[0] = false;
                    bombed[1] = true;
                }
                i++;
                await Task.Delay(150);
                win(i);
            }
            else if (i < 30)
            {
                bombed[0] = false;
                bombed[1] = false;
                i++;
                await Task.Delay(50);
                win(i);
            }
            else
            {
                gameState = 2;
            }
        }

        async void lose(int i)
        {
            if (!started)
                return;

            if (picY > 120)
            {
                whichPic = 13;
                picX += (picDir * 2 - 1) * 2;
                picY += (15 - i);

                i++;
                if (picY <= 125)
                    i = 20;
                await Task.Delay(30);
                lose(i);
            }
            else if (i < 50)
            {
                picY = 120;
                whichPic = 14;
                i++;
                await Task.Delay(50);
                lose(i);
            }
            else
            {
                gameState = 1;
            }
        }

        void bossFire()
        {
            if (fired[7] == 0)
            {
                if (bossY == 120)
                    whichBoss = 4;
                if (bossDir == 0)
                {
                    buX[7] = bossX - 10;
                    buDir[7] = 0;
                }
                else
                {
                    buX[7] = bossX - 10;
                    buDir[7] = 1;
                }
                buY[7] = bossY + 10;
                fired[7] = 1;
                fire(7);
            }
        }

        async void bossJump(int i)
        {
            if (bossLife <= 0)
            {
                update(0);
                return;
            }

            if (!started)
                return;

            if (i < 40)
            {
                whichBoss = 1;
                bossX -= dist / 40;
                bossY += 20 - i;
                i++;
                await Task.Delay(20);
                bossJump(i);
            }
            else
            {
                if (picX < bossX)
                    bossDir = 1;
                else
                    bossDir = 0;
                whichBoss = 0;
                bossY = 120;
                update(0);
            }
        }

        async void bossDash(int i)
        {
            if (bossLife <= 0)
            {
                update(0);
                return;
            }

            if (!started)
                return;

            whichBoss = 2;
            if (bossDir == 0)
            {
                if (bossX < i)
                {
                    bossX += 12;
                    await Task.Delay(20);
                    bossDash(i);
                }
                else
                {
                    whichBoss = 0;
                    if (picX < bossX)
                        bossDir = 1;
                    else
                        bossDir = 0;
                    update(0);
                }
            }
            else
            {
                if (bossX > i)
                {
                    bossX -= 12;
                    await Task.Delay(20);
                    bossDash(i);
                }
                else
                {
                    whichBoss = 0;
                    if (picX < bossX)
                        bossDir = 1;
                    else
                        bossDir = 0;
                    update(0);
                }
            }
        }

        async void bossUlt(int i)
        {
            if (bossLife <= 0)
            {
                update(0);
                return;
            }

            if (!started)
            {
                for (int j = 0; j < 5; j++)
                {
                    waved[j] = false;
                    waveY[j] = 700;
                }
                return;
            }

            if (i < 100 && waveY[0] > 120)
            {
                whichBoss = 3;
                for (int j = 0; j < 5; j++)
                    waveY[j] -= 10;

                i++;
                await Task.Delay(20);
                bossUlt(i);
            }
            else
            {
                for (int j = 0; j < 5; j++)
                {
                    waved[j] = false;
                    waveY[j] = 700;
                }
                whichBoss = 0;
                update(0);
            }
        }

        async void update(int i)
        {
            if (bossLife <= 0)
            {
                for (int j = 0; j < 2; j++)
                {
                    bombX[j] = bossX;
                    bombY[j] = bossY;
                }
                totalTime = prevTime - startTime;
                win(0);
                return;
            }
            if (!started)
                return;
            if (i < 40)
            {
                if (i == 19)
                    bossFire();
                i++;
                await Task.Delay(50);
                update(i);
            }
            else
            {
                Random rnd = new Random(Guid.NewGuid().GetHashCode());
                int r = rnd.Next(8);
                if (r <= 2)
                {
                    if (picX < bossX)
                    {
                        dist = bossX - picX / 2;
                        if (dist < 0) dist = 10;
                        bossDir = 1;
                    }
                    else
                    {
                        dist = bossX - (picX + 750) / 2;
                        if (dist > 0) dist = 760;
                        bossDir = 0;
                    }
                    bossJump(0);
                }
                else if (r > 2 && r <= 5)
                {
                    if (picX < bossX)
                        bossDash(picX / 2);
                    else
                        bossDash((picX + 750) / 2);
                }
                else
                {
                    for (int j = 0; j < 5; j++)
                    {
                        waved[j] = true;
                        waveX[j] = j * 180;
                        waveY[j] = 700;
                    }
                    bossUlt(0);
                }
            }
        }

        void restart()
        {
            whichPic = 0;
            whichBoss = 0;
            picX = 100; picY = 120;
            bossX = 600; bossY = 120;
            bossLife = 40;
            for (int j = 0; j < 5; j++)
            {
                waved[j] = false;
                waveY[j] = 700;
            }
            for (int j = 0; j < 8; j++)
            {
                fired[j] = 0;
                buY[j] = 700;
            }
            movingL = false; movingR = false; dead = false; vani = false;
            bombed[0] = false;
            bombed[1] = false;
            started = false;
            gameState = 0;
            totalTime = 0;
            jumpState = 0; dashState = 0;
            picDir = 0; bossDir = 1;
        }

        void start()
        {
            if (gameState == 0)
            {
                startTime = DateTime.Now.Second;
                prevTime = startTime;
                started = true;
                update(0);
            }
        }

        void KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key) {
                case Key.Left:
		            movingR=false;
			        if(!movingL && started){
                        movingL=true;
                        moveLeft(0);
		        	}
	        		break;
		        case Key.Right:
                    movingL=false;
			        if(!movingR && started){
                        movingR=true;
                        moveRight(0);
			        }
			        break;
                case Key.S:
                    if(gameState==0){
                        startTime = DateTime.Now.Second;
                        prevTime = startTime;
                        started=true;
                        update(0);
                    }
	                break;
                /*case Key.Q:
			        exit(0);
			        break;*/
                case Key.R:
                    restart();
                    update(0);
                    break;
		        case Key.X:
			        if(jumpState==0 && !dead && started) {
				        jumpState=1;
				        jump(0);
			        }
			        break;
                case Key.Z:
                    if(dashState==0 && !dead && started) {
				        dashState=1;
				        dash(0);
			        }
			        break;
                case Key.C:
                case Key.Space:
                    for(int i=0;i<7;i++){
                        if(fired[i]==0 && !dead && started){
                            if (whichPic==0 || whichPic==6) whichPic=6;
                            else if(whichPic==4 || whichPic==7) whichPic=7;
                            else if(whichPic<9 && whichPic!=5) whichPic=9;

                            if(picDir==0){
                                buX[i]=picX+(int)(pic[whichPic].Width*2.5)-10;
                                buDir[i]=0;
                            }
                            else{
                                buX[i]=picX-10;
                                buDir[i]=1;
                            }
                            if(whichPic!=7)
                                buY[i]=picY+20;
                            else
                                buY[i]=picY+50;
                            fired[i]=1;
                            for(int j=0;j<7;j++)
                                if(fired[j]==0)   fired[j]=2;
                            fire(i);
                            break;
                        }
                    }
			        break;
            }
        }
        void KeyUp(object sender, KeyEventArgs e)
        {
            switch(e.Key){
                case Key.Left:
                    movingL=false;
                    break;
                case Key.Right:
                    movingR=false;
                    break;
                case Key.C:
                case Key.Space:
                    for(int i=0;i<7;i++)
                        if(fired[i]==2) fired[i]=0;
                    break;

            }
        }

        void Media_Ended(object sender, EventArgs e)
        {
            bgm.Position = TimeSpan.FromSeconds(0);
            bgm.Play();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            restart();
            update(0);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.Show();
        }

        private void MenuItem_Click2(object sender, RoutedEventArgs e)
        {
            start();
        }
    }
}
