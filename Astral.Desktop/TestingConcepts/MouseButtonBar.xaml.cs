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

namespace TestingConcepts
{
    public enum MouseButtonBarState
    {
        Down,
        Up,
        Move,
        Scroll
    }

    public class MouseButtonBarEventArgs : EventArgs
    {
        public MouseButtonBarState State { get; set; }

        public MouseButtonBarEventArgs(MouseButtonBarState state)
        {
            this.State = state;
        }
    }

    /// <summary>
    /// Interaction logic for MouseButtonBar.xaml
    /// </summary>
    public partial class MouseButtonBar : UserControl
    {
        private Rectangle selected;
        private MouseButtonBarState currentState = MouseButtonBarState.Scroll;

        public event EventHandler<MouseButtonBarEventArgs> Selection;

        public MouseButtonBarState CurrentMouseBarState
        {
            get
            {
                return this.currentState;
            }
        }

        private void RaiseSelection(MouseButtonBarEventArgs e)
        {
            if(Selection != null)
            {
                Selection(this, e);
            }
        }
        
        public MouseButtonBar()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;

        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.DownRect.MouseEnter += OnRectEntered;
            this.UpRect.MouseEnter += OnRectEntered;
            this.MoveRect.MouseEnter += OnRectEntered;
            this.ScrollRect.MouseEnter += OnRectEntered;

            this.DownRect.MouseLeave += OnRectLeft;
            this.UpRect.MouseLeave += OnRectLeft;
            this.MoveRect.MouseLeave += OnRectLeft;
            this.ScrollRect.MouseLeave += OnRectLeft;

            this.DownRect.MouseLeftButtonDown += OnLeftButtonDown;
            this.UpRect.MouseLeftButtonDown += OnLeftButtonDown;
            this.MoveRect.MouseLeftButtonDown += OnLeftButtonDown;
            this.ScrollRect.MouseLeftButtonDown += OnLeftButtonDown;

            this.DownText.MouseLeftButtonDown += OnLeftButtonDownText;
            this.UpText.MouseLeftButtonDown += OnLeftButtonDownText;
            this.MoveText.MouseLeftButtonDown += OnLeftButtonDownText;
            this.ScrollText.MouseLeftButtonDown += OnLeftButtonDownText;

            this.DownText.MouseEnter += TextMouseEnter;
            this.UpText.MouseEnter += TextMouseEnter;
            this.MoveText.MouseEnter += TextMouseEnter;
            this.ScrollText.MouseEnter += TextMouseEnter;

            this.DownText.MouseLeave += TextMouseLeave;
            this.UpText.MouseLeave += TextMouseLeave;
            this.MoveText.MouseLeave += TextMouseLeave;
            this.ScrollText.MouseLeave += TextMouseLeave;

            this.selected = this.ScrollRect;
            DetermineSelection(this.selected);
        }

        private void TextMouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock hover = sender as TextBlock;
            if (hover == this.ScrollText)
            {
                this.ScrollRect.Fill = AstralColors.Yellow;
            }
            else if (hover == this.UpText)
            {
                this.UpRect.Fill = AstralColors.Yellow;
            }
            else if (hover == this.DownText)
            {
                this.DownRect.Fill = AstralColors.Yellow;
            }
            else if (hover == this.MoveText)
            {
                this.MoveRect.Fill = AstralColors.Yellow;
            }
        }

        private void TextMouseLeave(object sender, MouseEventArgs e)
        {
            ResetButtons();
            this.selected.Fill = AstralColors.Orange;
        }

        private void DetermineSelection(UIElement selection)
        {
            if(selection == this.DownRect || selection == this.DownText)
            {
                this.currentState = MouseButtonBarState.Down;
                this.selected = this.DownRect;
                this.DownRect.Fill = AstralColors.Orange;
            }
            else if(selection == this.UpRect || selection == this.UpText)
            {
                this.currentState = MouseButtonBarState.Up;
                this.selected = this.UpRect;
                this.UpRect.Fill = AstralColors.Orange;
            }
            else if(selection == this.MoveRect || selection == this.MoveText)
            {
                this.currentState = MouseButtonBarState.Move;
                this.selected = this.MoveRect;
                this.MoveRect.Fill = AstralColors.Orange;
            }
            else if(selection == this.ScrollRect || selection == this.ScrollText)
            {
                this.currentState = MouseButtonBarState.Scroll;
                this.selected = this.ScrollRect;
                this.ScrollRect.Fill = AstralColors.Orange;
            }
            RaiseSelection(new MouseButtonBarEventArgs(this.currentState));
        }

        private void ResetButtons()
        {
            this.DownRect.Fill = AstralColors.Teal;
            this.UpRect.Fill = AstralColors.Teal;
            this.MoveRect.Fill = AstralColors.Teal;
            this.ScrollRect.Fill = AstralColors.Teal;
            this.selected.Fill = AstralColors.Orange;
        }

        private void OnLeftButtonDownText(object sender, MouseButtonEventArgs e)
        {
            DetermineSelection(sender as UIElement);
            ResetButtons();

        }

        private void OnLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle clicked = sender as Rectangle;
            this.selected = clicked;
            ResetButtons();
            clicked.Fill = AstralColors.Orange;
            DetermineSelection(sender as UIElement);
        }

        private void OnRectLeft(object sender, MouseEventArgs e)
        {
            ResetButtons();
            selected.Fill = AstralColors.Orange;
            
        }

        private void OnRectEntered(object sender, MouseEventArgs e)
        {
            Rectangle hover = sender as Rectangle;
            hover.Fill = AstralColors.Yellow;
        }
    }
}
