import os
import time

# Kill any running instance
os.system('taskkill /F /IM accountant.exe 2>nul')
time.sleep(1)

sidebar_file = r'C:\Users\musta\Desktop\pro\accountant\Presentation\Controls\SidebarControl.cs'

# Read the file
with open(sidebar_file, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the MenuItem_Click method to check if enabled
old_click = '''    private void MenuItem_Click(object? sender, EventArgs e)
    {
        if (sender is SidebarMenuItem clickedItem)
        {
            // Deactivate previous item
            _activeItem?.SetActive(false);

            // Activate clicked item
            clickedItem.SetActive(true);
            _activeItem = clickedItem;

            // Raise event
            MenuItemClicked?.Invoke(this, clickedItem.Id);
        }
    }'''

new_click = '''    private void MenuItem_Click(object? sender, EventArgs e)
    {
        if (sender is SidebarMenuItem clickedItem)
        {
            // Check if item is enabled
            if (!clickedItem.IsEnabled)
            {
                MessageBox.Show(
                    "ليس لديك صلاحيات للوصول إلى هذا القسم.\\nالرجاء التواصل مع المسؤول للحصول على الصلاحيات المطلوبة.",
                    "صلاحيات غير كافية",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                );
                return;
            }

            // Deactivate previous item
            _activeItem?.SetActive(false);

            // Activate clicked item
            clickedItem.SetActive(true);
            _activeItem = clickedItem;

            // Raise event
            MenuItemClicked?.Invoke(this, clickedItem.Id);
        }
    }'''

content = content.replace(old_click, new_click)

# Add _isEnabled field to SidebarMenuItem
old_class_start = '''public class SidebarMenuItem : Panel
{
    public string Id { get; }
    private readonly Label _iconLabel;
    private readonly Label _textLabel;
    private Label? _badgeLabel;
    private bool _isActive;'''

new_class_start = '''public class SidebarMenuItem : Panel
{
    public string Id { get; }
    private readonly Label _iconLabel;
    private readonly Label _textLabel;
    private Label? _badgeLabel;
    private bool _isActive;
    private bool _isEnabled = true;'''

content = content.replace(old_class_start, new_class_start)

# Update hover effects
old_hover = '''        // Hover effects
        this.MouseEnter += (s, e) =>
        {
            if (!_isActive)
                this.BackColor = ColorScheme.SidebarHover;
        };

        this.MouseLeave += (s, e) =>
        {
            if (!_isActive)
                this.BackColor = ColorScheme.SidebarBg;
        };'''

new_hover = '''        // Hover effects
        this.MouseEnter += (s, e) =>
        {
            if (_isEnabled && !_isActive)
                this.BackColor = ColorScheme.SidebarHover;
        };

        this.MouseLeave += (s, e) =>
        {
            if (!_isActive)
            {
                if (_isEnabled)
                    this.BackColor = ColorScheme.SidebarBg;
                else
                    this.BackColor = Color.FromArgb(40, 40, 40);
            }
        };'''

content = content.replace(old_hover, new_hover)

# Add SetEnabled method and IsEnabled property
old_setactive = '''    public void SetActive(bool isActive)
    {
        _isActive = isActive;
        
        if (isActive)
        {
            this.BackColor = ColorScheme.Primary;
            _textLabel.Font = new Font("Cairo", 10F, FontStyle.Bold); // تقليل من 11 إلى 10
        }
        else
        {
            this.BackColor = ColorScheme.SidebarBg;
            _textLabel.Font = new Font("Cairo", 10F, FontStyle.Regular); // تقليل من 11 إلى 10
        }
    }'''

new_setactive = '''    public void SetEnabled(bool enabled)
    {
        _isEnabled = enabled;
        
        if (enabled)
        {
            // Enabled state - normal colors
            _iconLabel.ForeColor = Color.White;
            _textLabel.ForeColor = Color.White;
            this.Cursor = Cursors.Hand;
            
            if (!_isActive)
                this.BackColor = ColorScheme.SidebarBg;
        }
        else
        {
            // Disabled state - grayed out
            _iconLabel.ForeColor = Color.FromArgb(100, 100, 100);
            _textLabel.ForeColor = Color.FromArgb(100, 100, 100);
            this.BackColor = Color.FromArgb(40, 40, 40);
            this.Cursor = Cursors.No;
        }
    }
    
    public bool IsEnabled => _isEnabled;

    public void SetActive(bool isActive)
    {
        _isActive = isActive;
        
        if (isActive && _isEnabled)
        {
            this.BackColor = ColorScheme.Primary;
            _textLabel.Font = new Font("Cairo", 10F, FontStyle.Bold);
        }
        else if (_isEnabled)
        {
            this.BackColor = ColorScheme.SidebarBg;
            _textLabel.Font = new Font("Cairo", 10F, FontStyle.Regular);
        }
        else
        {
            // Keep disabled styling
            this.BackColor = Color.FromArgb(40, 40, 40);
            _textLabel.Font = new Font("Cairo", 10F, FontStyle.Regular);
        }
    }'''

content = content.replace(old_setactive, new_setactive)

# Write the modified content
with open(sidebar_file, 'w', encoding='utf-8') as f:
    f.write(content)

print("Success! SidebarControl.cs updated")
print("Changes:")
print("1. IsEnabled check in MenuItem_Click")
print("2. _isEnabled field added")
print("3. SetEnabled() method added")
print("4. IsEnabled property added")
print("5. Hover effects updated")
print("6. SetActive() updated")
