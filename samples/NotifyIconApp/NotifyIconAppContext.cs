using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.Messaging;
using NotifyIconApp.Properties;

namespace NotifyIconApp;

class NotifyIconAppContext : ApplicationContext, IRecipient<ShowBalloonTipMessage>
{
    IContainer _components;
    NotifyIcon _notifyIcon;

    public NotifyIconAppContext(IMessenger messenger)
    {
        InitializeComponent();
        messenger.RegisterAll(this);
    }

    [MemberNotNull(nameof(_components))]
    [MemberNotNull(nameof(_notifyIcon))]
    void InitializeComponent()
    {
        _components = new Container();

        _notifyIcon = new NotifyIcon(_components);
        var contextMenuStrip = new ContextMenuStrip(_components);
        contextMenuStrip.SuspendLayout();

        _notifyIcon.ContextMenuStrip = contextMenuStrip;
        _notifyIcon.Icon = Resources.Icon;
        _notifyIcon.Text = "NotifyIconApp";
        _notifyIcon.Visible = true;
        _notifyIcon.BalloonTipClicked += NotifyIcon_BalloonTipClicked; ;
        _notifyIcon.DoubleClick += NotifyIcon_DoubleClick; ;

        contextMenuStrip.Items.AddRange(new[]
        {
            new ToolStripMenuItem("E&xit", null, Exit_Click)
        });

        contextMenuStrip.ResumeLayout(performLayout: false);
    }

    public void Receive(ShowBalloonTipMessage message)
    {
        _notifyIcon.ShowBalloonTip(0, message.Title, message.Text, message.Icon);
    }

    void NotifyIcon_BalloonTipClicked(object? sender, EventArgs e)
    {
        // TODO
    }

    void NotifyIcon_DoubleClick(object? sender, EventArgs e)
    {
        // TODO
    }

    void Exit_Click(object? sender, EventArgs e)
    {
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _components.Dispose();
        }

        base.Dispose(disposing);
    }
}

