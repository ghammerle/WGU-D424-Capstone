namespace C424Assessment.Controls;

public partial class ButtonView : ContentView
{
    public event EventHandler<EventArgs> OnSave;
    public event EventHandler<EventArgs> OnCancel;
    public event EventHandler<EventArgs> OnDelete;

    public static readonly BindableProperty IsCancelVisibleProperty =
    BindableProperty.Create(nameof(IsCancelVisible), typeof(bool), typeof(ButtonView), true);

    public bool IsCancelVisible
    {
        get => (bool)GetValue(IsCancelVisibleProperty);
        set => SetValue(IsCancelVisibleProperty, value);
    }

    public ButtonView()
	{
		InitializeComponent();
	}

    private void editButton_Clicked(object sender, EventArgs e)
    {
        OnSave?.Invoke(sender, e);
    }

    private void cancelButton_Clicked(object sender, EventArgs e)
    {
        OnCancel?.Invoke(sender, e);
    }

    private void DeleteButton_Clicked(object sender, EventArgs e)
    {
        OnDelete?.Invoke(sender, e);
    }
}