using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using GraceWay.AccountingSystem.Presentation;

namespace GraceWay.AccountingSystem.Presentation.Forms;

/// <summary>
/// آلة حاسبة محاسبية احترافية
/// </summary>
public partial class AccountingCalculatorForm : Form
{
    // Core calculation variables
    private decimal _currentValue = 0;
    private decimal _previousValue = 0;
    private string _currentOperation = "";
    private bool _isNewEntry = true;
    private decimal _memory = 0;
    
    // History tracking
    private List<string> _historyList = new();
    private decimal _taxRate = 0.14m; // 14% default tax rate
    
    // UI Controls
    private TextBox _displayBox = null!;
    private TextBox _expressionBox = null!;
    private ListBox _historyListBox = null!;
    private Label _memoryIndicator = null!;
    private Panel _displayPanel = null!;
    private Panel _buttonPanel = null!;
    private Panel _historyPanel = null!;
    private Panel _memoryPanel = null!;
    
    // Buttons
    private Dictionary<string, Button> _buttons = new();
    
    public AccountingCalculatorForm()
    {
        InitializeComponent();
        SetupForm();
        InitializeCustomControls();
        SetupKeyboardShortcuts();
    }
    
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        // Set focus to display box when form is shown
        _displayBox.Focus();
    }
    
    private void SetupForm()
    {
        this.Text = "الآلة الحاسبة المحاسبية";
        this.Size = new Size(900, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;
        this.BackColor = ColorScheme.Background;
        this.Font = new Font("Cairo", 10F);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.KeyPreview = true;
    }
    
    private void InitializeCustomControls()
    {
        // ══════════════════════════════════════
        // Main Layout - Split into 3 sections
        // ══════════════════════════════════════
        
        // Right Panel - Calculator (60%)
        Panel calculatorMainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            BackColor = Color.White
        };
        
        // Left Panel - History & Memory (40%)
        Panel sidePanel = new Panel
        {
            Dock = DockStyle.Right,
            Width = 320,
            BackColor = ColorScheme.LightGray,
            Padding = new Padding(10)
        };
        
        // ══════════════════════════════════════
        // Display Panel
        // ══════════════════════════════════════
        _displayPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 140,
            BackColor = Color.FromArgb(245, 245, 245),
            Padding = new Padding(15)
        };
        
        // Expression Box (shows calculation)
        _expressionBox = new TextBox
        {
            Dock = DockStyle.Top,
            Height = 35,
            Font = new Font("Consolas", 11F),
            BorderStyle = BorderStyle.None,
            BackColor = Color.FromArgb(245, 245, 245),
            ForeColor = ColorScheme.TextSecondary,
            ReadOnly = true,
            TextAlign = HorizontalAlignment.Left,
            Text = "0"
        };
        
        // Main Display Box
        _displayBox = new TextBox
        {
            Dock = DockStyle.Bottom,
            Height = 65,
            Font = new Font("Consolas", 28F, FontStyle.Bold),
            BorderStyle = BorderStyle.None,
            BackColor = Color.FromArgb(245, 245, 245),
            ForeColor = Color.FromArgb(33, 33, 33),
            ReadOnly = true,
            TextAlign = HorizontalAlignment.Left,
            Text = "0",
            TabStop = true,
            TabIndex = 0
        };
        
        // Memory Indicator
        _memoryIndicator = new Label
        {
            Text = "",
            Font = new Font("Cairo", 9F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            AutoSize = true,
            Location = new Point(15, 10),
            BackColor = Color.Transparent
        };
        
        _displayPanel.Controls.Add(_displayBox);
        _displayPanel.Controls.Add(_expressionBox);
        _displayPanel.Controls.Add(_memoryIndicator);
        
        // ══════════════════════════════════════
        // Button Panel
        // ══════════════════════════════════════
        _buttonPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            BackColor = Color.White
        };
        
        CreateCalculatorButtons();
        
        // ══════════════════════════════════════
        // History Panel
        // ══════════════════════════════════════
        _historyPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 330,
            Padding = new Padding(5),
            BackColor = Color.White
        };
        
        Label historyLabel = new Label
        {
            Text = "📜 سجل العمليات",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Dock = DockStyle.Top,
            Height = 35,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 5, 5, 0)
        };
        
        _historyListBox = new ListBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 9.5F),
            BorderStyle = BorderStyle.None,
            BackColor = Color.White,
            SelectionMode = SelectionMode.One
        };
        _historyListBox.DoubleClick += HistoryItem_DoubleClick;
        
        Button clearHistoryBtn = new Button
        {
            Text = "🗑️ مسح السجل",
            Dock = DockStyle.Bottom,
            Height = 35,
            Font = new Font("Cairo", 9F),
            BackColor = ColorScheme.Error,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        clearHistoryBtn.FlatAppearance.BorderSize = 0;
        clearHistoryBtn.Click += (s, e) =>
        {
            _historyList.Clear();
            _historyListBox.Items.Clear();
        };
        
        _historyPanel.Controls.Add(_historyListBox);
        _historyPanel.Controls.Add(clearHistoryBtn);
        _historyPanel.Controls.Add(historyLabel);
        
        // ══════════════════════════════════════
        // Memory Panel
        // ══════════════════════════════════════
        _memoryPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(5),
            BackColor = Color.White
        };
        
        Label memoryLabel = new Label
        {
            Text = "💾 الذاكرة",
            Font = new Font("Cairo", 10F, FontStyle.Bold),
            ForeColor = ColorScheme.Primary,
            Dock = DockStyle.Top,
            Height = 35,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 5, 5, 0)
        };
        
        // Memory value display
        Label memoryValueLabel = new Label
        {
            Text = "القيمة المحفوظة: 0",
            Font = new Font("Consolas", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33),
            Dock = DockStyle.Top,
            Height = 40,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.FromArgb(245, 245, 245),
            Margin = new Padding(5)
        };
        memoryValueLabel.Tag = "memoryValue";
        
        // Memory buttons grid
        Panel memoryButtonsPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 200,
            Padding = new Padding(5)
        };
        
        CreateMemoryButtons(memoryButtonsPanel, memoryValueLabel);
        
        _memoryPanel.Controls.Add(memoryButtonsPanel);
        _memoryPanel.Controls.Add(memoryValueLabel);
        _memoryPanel.Controls.Add(memoryLabel);
        
        // ══════════════════════════════════════
        // Assemble Layout
        // ══════════════════════════════════════
        sidePanel.Controls.Add(_memoryPanel);
        sidePanel.Controls.Add(_historyPanel);
        
        calculatorMainPanel.Controls.Add(_buttonPanel);
        calculatorMainPanel.Controls.Add(_displayPanel);
        
        this.Controls.Add(calculatorMainPanel);
        this.Controls.Add(sidePanel);
    }
    
    private void CreateCalculatorButtons()
    {
        int buttonWidth = 70;
        int buttonHeight = 55;
        int spacing = 8;
        int startX = 15;
        int startY = 15;
        
        // Button Layout (Right to Left, Arabic style)
        string[][] buttonLayout = new string[][]
        {
            // Row 1: Memory & Special Functions
            new[] { "MC", "MR", "M+", "M-", "MS" },
            
            // Row 2: Functions
            new[] { "√", "x²", "%", "±", "CE" },
            
            // Row 3: Numbers & Operations
            new[] { "7", "8", "9", "÷", "C" },
            new[] { "4", "5", "6", "×", "⌫" },
            new[] { "1", "2", "3", "-", "TAX+" },
            new[] { "0", ".", "=", "+", "TAX-" }
        };
        
        for (int row = 0; row < buttonLayout.Length; row++)
        {
            for (int col = 0; col < buttonLayout[row].Length; col++)
            {
                string btnText = buttonLayout[row][col];
                int x = startX + col * (buttonWidth + spacing);
                int y = startY + row * (buttonHeight + spacing);
                
                Button btn = CreateStyledButton(btnText, x, y, buttonWidth, buttonHeight);
                _buttonPanel.Controls.Add(btn);
                _buttons[btnText] = btn;
            }
        }
    }
    
    private Button CreateStyledButton(string text, int x, int y, int width, int height)
    {
        Button btn = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            Font = new Font("Cairo", 11F, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            TabStop = false
        };
        
        // Color scheme based on button type
        if (char.IsDigit(text[0]) || text == ".")
        {
            // Number buttons - White
            btn.BackColor = Color.White;
            btn.ForeColor = Color.FromArgb(33, 33, 33);
            btn.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
        }
        else if (text == "=" || text == "C" || text == "CE")
        {
            // Equals and Clear - Primary color
            btn.BackColor = ColorScheme.Primary;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderSize = 0;
        }
        else if (text == "+" || text == "-" || text == "×" || text == "÷")
        {
            // Operations - Orange
            btn.BackColor = Color.FromArgb(255, 152, 0);
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderSize = 0;
        }
        else if (text.StartsWith("M") || text == "MS")
        {
            // Memory buttons - Purple
            btn.BackColor = Color.FromArgb(156, 39, 176);
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderSize = 0;
        }
        else if (text.StartsWith("TAX"))
        {
            // Tax buttons - Green/Red
            btn.BackColor = text.Contains("+") ? ColorScheme.Success : ColorScheme.Error;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Cairo", 8.5F, FontStyle.Bold);
        }
        else
        {
            // Function buttons - Gray
            btn.BackColor = Color.FromArgb(245, 245, 245);
            btn.ForeColor = Color.FromArgb(33, 33, 33);
            btn.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
        }
        
        btn.FlatAppearance.BorderSize = 1;
        
        // Hover effects
        Color originalColor = btn.BackColor;
        btn.MouseEnter += (s, e) =>
        {
            btn.BackColor = ColorScheme.Darken(originalColor, 0.1f);
        };
        btn.MouseLeave += (s, e) =>
        {
            btn.BackColor = originalColor;
        };
        
        // Click handler
        btn.Click += CalculatorButton_Click;
        
        return btn;
    }
    
    private void CreateMemoryButtons(Panel panel, Label valueLabel)
    {
        string[] memoryBtns = new[] { "MC", "MR", "M+", "M-", "MS" };
        string[] labels = new[] { "مسح", "استرجاع", "إضافة", "طرح", "حفظ" };
        
        int btnHeight = 35;
        int spacing = 5;
        int y = 0;
        
        for (int i = 0; i < memoryBtns.Length; i++)
        {
            Button btn = new Button
            {
                Text = $"{memoryBtns[i]} - {labels[i]}",
                Location = new Point(5, y),
                Size = new Size(panel.Width - 10, btnHeight),
                Font = new Font("Cairo", 9.5F, FontStyle.Bold),
                BackColor = Color.FromArgb(156, 39, 176),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Tag = memoryBtns[i];
            
            btn.Click += (s, e) =>
            {
                string action = ((Button)s!).Tag!.ToString()!;
                HandleMemoryOperation(action, valueLabel);
            };
            
            panel.Controls.Add(btn);
            y += btnHeight + spacing;
        }
    }
    
    private void SetupKeyboardShortcuts()
    {
        this.KeyDown += (s, e) =>
        {
            // Numbers
            if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
            {
                ProcessNumber((e.KeyCode - Keys.D0).ToString());
                e.Handled = true;
            }
            else if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9)
            {
                ProcessNumber((e.KeyCode - Keys.NumPad0).ToString());
                e.Handled = true;
            }
            // Operations (with Shift key requirement for Windows compatibility)
            else if (e.KeyCode == Keys.Add)
            {
                ProcessOperation("+");
                e.Handled = true;
            }
            else if (e.Shift && e.KeyCode == Keys.Oemplus) // Shift + = for +
            {
                ProcessOperation("+");
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
            {
                ProcessOperation("-");
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Multiply)
            {
                ProcessOperation("×");
                e.Handled = true;
            }
            else if (e.Shift && e.KeyCode == Keys.D8) // Shift + 8 for ×
            {
                ProcessOperation("×");
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Divide)
            {
                ProcessOperation("÷");
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.OemQuestion) // / for ÷
            {
                ProcessOperation("÷");
                e.Handled = true;
            }
            // Equals
            else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Oemplus && !e.Shift)
            {
                CalculateResult();
                e.Handled = true;
            }
            // Decimal
            else if (e.KeyCode == Keys.Decimal || e.KeyCode == Keys.OemPeriod)
            {
                ProcessNumber(".");
                e.Handled = true;
            }
            // Clear
            else if (e.KeyCode == Keys.Escape)
            {
                Clear();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                ClearEntry();
                e.Handled = true;
            }
            // Backspace
            else if (e.KeyCode == Keys.Back)
            {
                Backspace();
                e.Handled = true;
            }
            // Memory shortcuts
            else if (e.Control)
            {
                if (e.KeyCode == Keys.M)
                {
                    // Ctrl+M = MS (Memory Store)
                    HandleMemoryOperation("MS", null);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.R)
                {
                    // Ctrl+R = MR (Memory Recall)
                    HandleMemoryOperation("MR", null);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.P)
                {
                    // Ctrl+P = M+ (Memory Plus)
                    HandleMemoryOperation("M+", null);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Q)
                {
                    // Ctrl+Q = M- (Memory Minus)
                    HandleMemoryOperation("M-", null);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.L)
                {
                    // Ctrl+L = MC (Memory Clear)
                    HandleMemoryOperation("MC", null);
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.C)
                {
                    // Ctrl+C = Copy result
                    Clipboard.SetText(_displayBox.Text);
                    ShowTooltip("تم النسخ ✓");
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.V)
                {
                    // Ctrl+V = Paste
                    try
                    {
                        string text = Clipboard.GetText();
                        if (decimal.TryParse(text, out decimal value))
                        {
                            _currentValue = value;
                            UpdateDisplay();
                            _isNewEntry = true;
                        }
                    }
                    catch { }
                    e.Handled = true;
                }
            }
            // Percentage
            else if (e.KeyCode == Keys.D5 && e.Shift)
            {
                ProcessPercentage();
                e.Handled = true;
            }
        };
    }
    
    // ══════════════════════════════════════
    // Calculator Logic
    // ══════════════════════════════════════
    
    private void CalculatorButton_Click(object? sender, EventArgs e)
    {
        if (sender is not Button button) return;
        string text = button.Text;
        
        // Numbers and decimal
        if (char.IsDigit(text[0]) || text == ".")
        {
            ProcessNumber(text);
        }
        // Operations
        else if (text == "+" || text == "-" || text == "×" || text == "÷")
        {
            ProcessOperation(text);
        }
        // Equals
        else if (text == "=")
        {
            CalculateResult();
        }
        // Clear functions
        else if (text == "C")
        {
            Clear();
        }
        else if (text == "CE")
        {
            ClearEntry();
        }
        else if (text == "⌫")
        {
            Backspace();
        }
        // Special functions
        else if (text == "%")
        {
            ProcessPercentage();
        }
        else if (text == "±")
        {
            ProcessSignChange();
        }
        else if (text == "√")
        {
            ProcessSquareRoot();
        }
        else if (text == "x²")
        {
            ProcessSquare();
        }
        // Tax functions
        else if (text == "TAX+")
        {
            ProcessTax(true);
        }
        else if (text == "TAX-")
        {
            ProcessTax(false);
        }
        // Memory functions
        else if (text.StartsWith("M") || text == "MS")
        {
            HandleMemoryOperation(text, null);
        }
    }
    
    private void ProcessNumber(string number)
    {
        if (_isNewEntry)
        {
            if (number == ".")
            {
                _displayBox.Text = "0.";
            }
            else
            {
                _displayBox.Text = number;
            }
            _isNewEntry = false;
        }
        else
        {
            if (number == ".")
            {
                if (!_displayBox.Text.Contains("."))
                {
                    _displayBox.Text += ".";
                }
            }
            else
            {
                if (_displayBox.Text == "0")
                {
                    _displayBox.Text = number;
                }
                else
                {
                    _displayBox.Text += number;
                }
            }
        }
        
        if (decimal.TryParse(_displayBox.Text, out decimal value))
        {
            _currentValue = value;
        }
    }
    
    private void ProcessOperation(string operation)
    {
        if (!string.IsNullOrEmpty(_currentOperation))
        {
            CalculateResult();
        }
        
        _previousValue = _currentValue;
        _currentOperation = operation;
        _expressionBox.Text = $"{FormatNumber(_previousValue)} {operation}";
        _isNewEntry = true;
    }
    
    private void CalculateResult()
    {
        if (string.IsNullOrEmpty(_currentOperation)) return;
        
        decimal result = 0;
        string expression = $"{FormatNumber(_previousValue)} {_currentOperation} {FormatNumber(_currentValue)}";
        
        try
        {
            result = _currentOperation switch
            {
                "+" => _previousValue + _currentValue,
                "-" => _previousValue - _currentValue,
                "×" => _previousValue * _currentValue,
                "÷" => _currentValue != 0 ? _previousValue / _currentValue : throw new DivideByZeroException(),
                _ => _currentValue
            };
            
            _currentValue = result;
            UpdateDisplay();
            
            // Add to history
            string historyEntry = $"{expression} = {FormatNumber(result)}";
            _historyList.Add(historyEntry);
            _historyListBox.Items.Insert(0, historyEntry);
            
            _expressionBox.Text = historyEntry;
            _currentOperation = "";
            _isNewEntry = true;
        }
        catch (DivideByZeroException)
        {
            ShowError("لا يمكن القسمة على صفر");
            Clear();
        }
        catch (Exception ex)
        {
            ShowError($"خطأ: {ex.Message}");
            Clear();
        }
    }
    
    private void ProcessPercentage()
    {
        if (_previousValue != 0 && !string.IsNullOrEmpty(_currentOperation))
        {
            // Calculate percentage of previous value
            _currentValue = (_previousValue * _currentValue) / 100;
            UpdateDisplay();
        }
        else
        {
            // Convert to percentage
            _currentValue = _currentValue / 100;
            UpdateDisplay();
            _isNewEntry = true;
        }
    }
    
    private void ProcessSignChange()
    {
        _currentValue = -_currentValue;
        UpdateDisplay();
    }
    
    private void ProcessSquareRoot()
    {
        if (_currentValue < 0)
        {
            ShowError("لا يمكن حساب الجذر التربيعي لرقم سالب");
            return;
        }
        
        decimal result = (decimal)Math.Sqrt((double)_currentValue);
        string expression = $"√({FormatNumber(_currentValue)})";
        _currentValue = result;
        UpdateDisplay();
        
        string historyEntry = $"{expression} = {FormatNumber(result)}";
        _historyList.Add(historyEntry);
        _historyListBox.Items.Insert(0, historyEntry);
        _expressionBox.Text = historyEntry;
        _isNewEntry = true;
    }
    
    private void ProcessSquare()
    {
        decimal result = _currentValue * _currentValue;
        string expression = $"({FormatNumber(_currentValue)})²";
        _currentValue = result;
        UpdateDisplay();
        
        string historyEntry = $"{expression} = {FormatNumber(result)}";
        _historyList.Add(historyEntry);
        _historyListBox.Items.Insert(0, historyEntry);
        _expressionBox.Text = historyEntry;
        _isNewEntry = true;
    }
    
    private void ProcessTax(bool add)
    {
        decimal tax = _currentValue * _taxRate;
        decimal result = add ? _currentValue + tax : _currentValue - tax;
        
        string operation = add ? "+" : "-";
        string expression = $"{FormatNumber(_currentValue)} {operation} ضريبة {_taxRate:P0}";
        
        _currentValue = result;
        UpdateDisplay();
        
        string historyEntry = $"{expression} = {FormatNumber(result)}";
        _historyList.Add(historyEntry);
        _historyListBox.Items.Insert(0, historyEntry);
        _expressionBox.Text = historyEntry;
        _isNewEntry = true;
    }
    
    private void Clear()
    {
        _currentValue = 0;
        _previousValue = 0;
        _currentOperation = "";
        _expressionBox.Text = "0";
        UpdateDisplay();
        _isNewEntry = true;
    }
    
    private void ClearEntry()
    {
        _currentValue = 0;
        UpdateDisplay();
        _isNewEntry = true;
    }
    
    private void Backspace()
    {
        if (_displayBox.Text.Length > 1)
        {
            _displayBox.Text = _displayBox.Text[..^1];
            if (decimal.TryParse(_displayBox.Text, out decimal value))
            {
                _currentValue = value;
            }
        }
        else
        {
            _displayBox.Text = "0";
            _currentValue = 0;
            _isNewEntry = true;
        }
    }
    
    // ══════════════════════════════════════
    // Memory Functions
    // ══════════════════════════════════════
    
    private void HandleMemoryOperation(string operation, Label? valueLabel)
    {
        switch (operation)
        {
            case "MC": // Memory Clear
                _memory = 0;
                UpdateMemoryIndicator();
                UpdateMemoryValueLabel(valueLabel);
                ShowTooltip("تم مسح الذاكرة");
                break;
                
            case "MR": // Memory Recall
                _currentValue = _memory;
                UpdateDisplay();
                _isNewEntry = true;
                ShowTooltip("تم استرجاع القيمة من الذاكرة");
                break;
                
            case "M+": // Memory Plus
                _memory += _currentValue;
                UpdateMemoryIndicator();
                UpdateMemoryValueLabel(valueLabel);
                ShowTooltip($"تمت إضافة {FormatNumber(_currentValue)}");
                break;
                
            case "M-": // Memory Minus
                _memory -= _currentValue;
                UpdateMemoryIndicator();
                UpdateMemoryValueLabel(valueLabel);
                ShowTooltip($"تم طرح {FormatNumber(_currentValue)}");
                break;
                
            case "MS": // Memory Store
                _memory = _currentValue;
                UpdateMemoryIndicator();
                UpdateMemoryValueLabel(valueLabel);
                ShowTooltip("تم حفظ القيمة في الذاكرة");
                break;
        }
    }
    
    private void UpdateMemoryIndicator()
    {
        _memoryIndicator.Text = _memory != 0 ? "💾 M" : "";
    }
    
    private void UpdateMemoryValueLabel(Label? label)
    {
        if (label != null)
        {
            label.Text = $"القيمة المحفوظة: {FormatNumber(_memory)}";
        }
        
        // Also update in memory panel if exists
        foreach (Control ctrl in _memoryPanel.Controls)
        {
            if (ctrl is Label lbl && lbl.Tag?.ToString() == "memoryValue")
            {
                lbl.Text = $"القيمة المحفوظة: {FormatNumber(_memory)}";
                break;
            }
        }
    }
    
    // ══════════════════════════════════════
    // History Functions
    // ══════════════════════════════════════
    
    private void HistoryItem_DoubleClick(object? sender, EventArgs e)
    {
        if (_historyListBox.SelectedItem == null) return;
        
        string item = _historyListBox.SelectedItem.ToString()!;
        
        // Extract the result from history entry (after "= ")
        int equalIndex = item.LastIndexOf("= ");
        if (equalIndex >= 0)
        {
            string resultStr = item[(equalIndex + 2)..].Replace(",", "").Trim();
            if (decimal.TryParse(resultStr, out decimal value))
            {
                _currentValue = value;
                UpdateDisplay();
                _isNewEntry = true;
                ShowTooltip("تم استرجاع النتيجة");
            }
        }
    }
    
    // ══════════════════════════════════════
    // Helper Functions
    // ══════════════════════════════════════
    
    private void UpdateDisplay()
    {
        _displayBox.Text = FormatNumber(_currentValue);
    }
    
    private string FormatNumber(decimal value)
    {
        // Format with thousands separator and up to 10 decimal places
        string formatted = value.ToString("N10");
        // Remove trailing zeros after decimal point
        formatted = formatted.TrimEnd('0').TrimEnd('.');
        return formatted;
    }
    
    private void ShowError(string message)
    {
        _displayBox.Text = "خطأ";
        _expressionBox.Text = message;
        _expressionBox.ForeColor = ColorScheme.Error;
        
        // Reset color after 2 seconds
        var timer = new System.Windows.Forms.Timer { Interval = 2000 };
        timer.Tick += (s, e) =>
        {
            _expressionBox.ForeColor = ColorScheme.TextSecondary;
            timer.Stop();
            timer.Dispose();
        };
        timer.Start();
        
        SystemSounds.Beep.Play();
    }
    
    private void ShowTooltip(string message)
    {
        // Create temporary tooltip label
        Label tooltip = new Label
        {
            Text = message,
            AutoSize = true,
            BackColor = Color.FromArgb(50, 50, 50),
            ForeColor = Color.White,
            Font = new Font("Cairo", 9F),
            Padding = new Padding(10, 5, 10, 5),
            Location = new Point(this.Width / 2 - 100, this.Height - 80)
        };
        
        this.Controls.Add(tooltip);
        tooltip.BringToFront();
        
        // Auto remove after 1.5 seconds
        var timer = new System.Windows.Forms.Timer { Interval = 1500 };
        timer.Tick += (s, e) =>
        {
            this.Controls.Remove(tooltip);
            tooltip.Dispose();
            timer.Stop();
            timer.Dispose();
        };
        timer.Start();
    }
}
