﻿namespace UWinForms.System.Windows.Forms
{
    using Drawing;
    using global::System;
    using global::System.Globalization;

    public class NumericUpDown : Control
    {
        internal Pen borderPen = new Pen(Color.Transparent);
        internal Button uwfButtonDecrease;
        internal Button uwfButtonIncrease;
        internal Color uwfDisabledColor;

        protected decimal value;
        protected string valueText = "0";

        private static readonly Padding defaultPadding = new Padding(4, 0, 4, 0);
        private decimal minimum;
        private decimal maximum;

        public NumericUpDown() : this(true)
        {
        }
        internal NumericUpDown(bool initButtons)
        {
            BackColor = SystemColors.Window;
            Increment = 1;
            Maximum = 100;
            Minimum = 0;
            Padding = defaultPadding;
            TextAlign = HorizontalAlignment.Left;

            uwfBorderColor = SystemColors.ActiveBorder;
            uwfDisabledColor = SystemColors.Control;

            if (initButtons)
            {
                uwfButtonIncrease = new UpDownButton(this, true);
                uwfButtonIncrease.Location = new Point(Width - 16, Height / 2 - 8);
                uwfButtonIncrease.Image = uwfAppOwner.Resources.NumericUp;

                uwfButtonDecrease = new UpDownButton(this, false);
                uwfButtonDecrease.Location = new Point(Width - 16, Height / 2);
                uwfButtonDecrease.Image = uwfAppOwner.Resources.NumericDown;

                Controls.Add(uwfButtonIncrease);
                Controls.Add(uwfButtonDecrease);
            }
        }

        public event EventHandler ValueChanged = delegate { };

        public decimal Increment { get; set; }
        public decimal Maximum
        {
            get { return maximum; }
            set
            {
                if (value < minimum) minimum = value;
                maximum = value;
                if (this.value < minimum) this.value = minimum;
                if (this.value > maximum) this.value = maximum;
            }
        }
        public decimal Minimum
        {
            get { return minimum; }
            set
            {
                if (value > maximum) maximum = value;
                minimum = value;
                if (this.value < minimum) this.value = minimum;
                if (this.value > maximum) this.value = maximum;
            }
        }
        public HorizontalAlignment TextAlign { get; set; }
        public decimal Value
        {
            get { return value; }
            set
            {
                if (this.value == value)
                    return;

                this.value = Constrain(value);
                valueText = this.value.ToString(Application.currentCulture);

                OnValueChanged(EventArgs.Empty);
            }
        }

        internal Color uwfBorderColor
        {
            get { return borderPen.Color; }
            set { borderPen.Color = value; }
        }

        protected override Size DefaultSize
        {
            get { return new Size(120, 20); }
        }

        public virtual void DownButton()
        {
            Value -= Increment;
        }
        public virtual void UpButton()
        {
            Value += Increment;
        }

        protected internal override void uwfOnLatePaint(PaintEventArgs e)
        {
            base.uwfOnLatePaint(e);

            e.Graphics.DrawRectangle(borderPen, 0, 0, Width, Height);
        }

        protected void ConfirmValue()
        {
            decimal local;
            if (decimal.TryParse(valueText, NumberStyles.Any, Application.currentCulture, out local))
                Value = local;
        }
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            ConfirmValue();
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Return)
                ConfirmValue();
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!Focused) return;

            if (e.Delta < 0)
                Value -= Increment;
            if (e.Delta > 0)
                Value += Increment;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

            int textPaddingRight = 0;
            if (uwfButtonIncrease != null && uwfButtonIncrease.Visible)
                textPaddingRight = -16;

            var backColor = Enabled ? BackColor : uwfDisabledColor;
            var foreColor = Enabled ? ForeColor : SystemColors.ActiveBorder;

            g.uwfFillRectangle(backColor, 0, 0, Width, Height);

            if (Focused)
                valueText = g.uwfDrawTextField(valueText, Font, foreColor, Padding.Left - 2, 0, Width + textPaddingRight + 4, Height, TextAlign);
            else
                g.uwfDrawString(valueText, Font, foreColor, Padding.Left, 0, Width + textPaddingRight, Height, TextAlign);
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            UpdateButtonsLocation();
        }
        protected virtual void OnValueChanged(EventArgs e)
        {
            if (ValueChanged != null)
                ValueChanged(this, e);
        }

        private decimal Constrain(decimal avalue)
        {
            if (avalue < minimum)
                avalue = minimum;

            if (avalue > maximum)
                avalue = maximum;

            return avalue;
        }
        private void UpdateButtonsLocation()
        {
            var width = Width;
            var height = Height;
            if (uwfButtonIncrease != null)
                uwfButtonIncrease.Location = new Point(width - 16, height / 2 - 8);
            if (uwfButtonDecrease != null)
                uwfButtonDecrease.Location = new Point(width - 16, height / 2);
        }

        internal class UpDownButton : RepeatButton
        {
            private static readonly Color defaultBorderColor = Color.FromArgb(172, 172, 172);
            private static readonly Color defaultBorderHoverColor = Color.FromArgb(126, 180, 234);
            private static readonly Color defaultHoverColor = Color.FromArgb(228, 241, 252);
            private readonly NumericUpDown owner;
            private readonly bool upButton;

            internal UpDownButton(NumericUpDown owner, bool upButton)
            {
                this.owner = owner;
                this.upButton = upButton;

                Anchor = AnchorStyles.Right | AnchorStyles.Top;
                BackColor = SystemColors.Control;
                TabStop = false;

                uwfBorderColor = defaultBorderColor;
                uwfHoverColor = defaultHoverColor;
                uwfBorderHoverColor = defaultBorderHoverColor;
            }

            protected override Size DefaultSize
            {
                get { return new Size(14, 8); }
            }

            protected override void OnClick(EventArgs e)
            {
                base.OnClick(e);

                if (upButton)
                    owner.UpButton();
                else
                    owner.DownButton();
            }
        }
    }
}
