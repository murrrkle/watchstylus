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
    public enum KeyboardButtonBarState
    {
        Down,
        Up,
        Press
    }

    public class KeyboardButtonBarEventArgs : EventArgs
    {
        public KeyboardButtonBarState State { get; set; }

        public KeyboardButtonBarEventArgs(KeyboardButtonBarState state)
        {
            this.State = state;
        }
    }


    /// <summary>
    /// Interaction logic for KeyboardButtonBar.xaml
    /// </summary>
    public partial class KeyboardButtonBar : UserControl
    {
        private Rectangle selected;
        private KeyboardButtonBarState currentState = KeyboardButtonBarState.Press;

        public event EventHandler<KeyboardButtonBarEventArgs> Selection;

        public KeyboardButtonBarState CurrentMouseBarState
        {
            get
            {
                return this.currentState;
            }
        }

        private void RaiseSelection(KeyboardButtonBarEventArgs e)
        {
            if (Selection != null)
            {
                Selection(this, e);
            }
        }

        public KeyboardButtonBar()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;

        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.DownRect.MouseEnter += OnRectEntered;
            this.UpRect.MouseEnter += OnRectEntered;
            this.MoveRect.MouseEnter += OnRectEntered;

            this.DownRect.MouseLeave += OnRectLeft;
            this.UpRect.MouseLeave += OnRectLeft;
            this.MoveRect.MouseLeave += OnRectLeft;

            this.DownRect.MouseLeftButtonDown += OnLeftButtonDown;
            this.UpRect.MouseLeftButtonDown += OnLeftButtonDown;
            this.MoveRect.MouseLeftButtonDown += OnLeftButtonDown;

            this.DownText.MouseLeftButtonDown += OnLeftButtonDownText;
            this.UpText.MouseLeftButtonDown += OnLeftButtonDownText;
            this.MoveText.MouseLeftButtonDown += OnLeftButtonDownText;

            this.DownText.MouseEnter += TextMouseEnter;
            this.UpText.MouseEnter += TextMouseEnter;
            this.MoveText.MouseEnter += TextMouseEnter;

            this.DownText.MouseLeave += TextMouseLeave;
            this.UpText.MouseLeave += TextMouseLeave;
            this.MoveText.MouseLeave += TextMouseLeave;

            this.selected = this.MoveRect;
            DetermineSelection(this.selected);
        }

        private void TextMouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock hover = sender as TextBlock;

            if (hover == this.UpText)
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
            if (selection == this.DownRect || selection == this.DownText)
            {
                this.currentState = KeyboardButtonBarState.Down;
                this.selected = this.DownRect;
                this.DownRect.Fill = AstralColors.Orange;
            }
            else if (selection == this.UpRect || selection == this.UpText)
            {
                this.currentState = KeyboardButtonBarState.Up;
                this.selected = this.UpRect;
                this.UpRect.Fill = AstralColors.Orange;
            }
            else if (selection == this.MoveRect || selection == this.MoveText)
            {
                this.currentState = KeyboardButtonBarState.Press;
                this.selected = this.MoveRect;
                this.MoveRect.Fill = AstralColors.Orange;
            }

            RaiseSelection(new KeyboardButtonBarEventArgs(this.currentState));
        }

        private void ResetButtons()
        {
            this.DownRect.Fill = AstralColors.Teal;
            this.UpRect.Fill = AstralColors.Teal;
            this.MoveRect.Fill = AstralColors.Teal;
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
