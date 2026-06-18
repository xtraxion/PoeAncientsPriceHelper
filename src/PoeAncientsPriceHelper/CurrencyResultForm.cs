using System.Drawing;
using System.Windows.Forms;

namespace PoeAncientsPriceHelper;

/// <summary>
/// Simple top-most result window for the F6 currency scan.  Shows a scrollable list of
/// recognised currencies with their divine/exalted prices, sorted by value descending.
/// Auto-closes after 10 s or on Esc / click outside (optional).
/// </summary>
internal sealed class CurrencyResultForm : Form
{
    private readonly List<PriceRow> _rows;
    private readonly IconCache _icons;
    private readonly System.Windows.Forms.Timer _closeTimer;
    private readonly Font _nameFont = new("Segoe UI", 11, FontStyle.Regular);
    private readonly Font _priceFont = new("Consolas", 11, FontStyle.Bold);
    private const int Pad = 8;
    private const int RowH = 34;
    private const int IconSize = 24;

    public CurrencyResultForm(List<PriceRow> rows, IconCache icons)
    {
        _rows = rows;
        _icons = icons;

        Text = "Currency Overview Prices";
        FormBorderStyle = FormBorderStyle.Sizable;
        StartPosition = FormStartPosition.Manual;
        Size = new Size(340, Math.Min(600, 80 + rows.Count * RowH));
        TopMost = true;
        ShowInTaskbar = false;
        BackColor = Color.FromArgb(32, 32, 38);
        ForeColor = Color.White;

        // Build UI
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = BackColor,
            Padding = new Padding(Pad),
        };

        int y = Pad;
        foreach (var row in _rows.OrderByDescending(r => r.DivineValue * Math.Max(1, r.Multiplier)))
        {
            var rowPanel = new Panel
            {
                Location = new Point(Pad, y),
                Size = new Size(Width - Pad * 2 - SystemInformation.VerticalScrollBarWidth, RowH),
                BackColor = Color.FromArgb(45, 45, 55),
            };

            bool useDivine = row.DivineValue >= 1.0m;
            decimal unit = useDivine ? row.DivineValue : row.ExaltedValue;
            int mult = Math.Max(1, row.Multiplier);
            decimal total = unit * mult;
            string fmt = useDivine ? "0.00" : "0.#";
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            string priceText = mult > 1
                ? $"{total.ToString(fmt, inv)} ({unit.ToString(fmt, inv)} each)"
                : total.ToString(fmt, inv);

            // Icon
            var icon = useDivine ? _icons.Divine : _icons.Exalted;
            if (icon is not null && _icons.IsAvailable)
            {
                var pic = new PictureBox
                {
                    Image = icon,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(IconSize, IconSize),
                    Location = new Point(6, (RowH - IconSize) / 2),
                    BackColor = Color.Transparent,
                };
                rowPanel.Controls.Add(pic);
            }

            // Name label
            var nameLbl = new Label
            {
                Text = row.Name,
                ForeColor = row.HasPrice ? Color.White : Color.Gray,
                Font = _nameFont,
                Location = new Point(6 + IconSize + 4, 4),
                Size = new Size(160, RowH - 8),
                AutoEllipsis = true,
            };
            rowPanel.Controls.Add(nameLbl);

            // Price label
            var priceLbl = new Label
            {
                Text = row.HasPrice ? priceText : "?",
                ForeColor = useDivine ? Color.Gold : Color.White,
                Font = _priceFont,
                Location = new Point(rowPanel.Width - 110, 6),
                Size = new Size(100, RowH - 8),
                TextAlign = ContentAlignment.MiddleRight,
            };
            rowPanel.Controls.Add(priceLbl);

            panel.Controls.Add(rowPanel);
            y += RowH + 4;
        }

        Controls.Add(panel);

        // Title bar: close button
        var closeBtn = new Button
        {
            Text = "×",
            ForeColor = Color.White,
            BackColor = Color.FromArgb(60, 60, 70),
            FlatStyle = FlatStyle.Flat,
            Size = new Size(28, 24),
            Location = new Point(Width - 40, 4),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
        };
        closeBtn.FlatAppearance.BorderSize = 0;
        closeBtn.Click += (_, _) => Close();
        Controls.Add(closeBtn);

        // Auto-close after 10 s
        _closeTimer = new System.Windows.Forms.Timer { Interval = 10000 };
        _closeTimer.Tick += (_, _) => { _closeTimer.Stop(); Close(); };
        _closeTimer.Start();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _closeTimer?.Stop();
        base.OnFormClosing(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape) { Close(); return; }
        base.OnKeyDown(e);
    }
}
