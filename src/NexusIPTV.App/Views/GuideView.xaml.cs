using System.Windows.Controls;
using NexusIPTV.Core.Models;

namespace NexusIPTV.App.Views;

public partial class GuideView : UserControl
{
    private Channel? _channel;

    public Programme? CurrentProgramme { get; private set; }

    public Programme? NextProgramme { get; private set; }

    public GuideView ()
    {
        InitializeComponent();
    }

    public void SetChannel ( Channel? channel )
    {
        _channel=channel;

        CurrentProgramme=null;
        NextProgramme=null;

        if ( channel is null )
        {
            CurrentTimeText.Text=string.Empty;
            CurrentProgrammeText.Text=
                "No programme information";
            CurrentDescriptionText.Text=string.Empty;

            NextTimeText.Text=string.Empty;
            NextProgrammeText.Text=
                "No programme information";
            NextDescriptionText.Text=string.Empty;

            return;
        }

        UpdateCurrentProgramme(DateTimeOffset.Now);
    }

    public void UpdateCurrentProgramme ( DateTimeOffset now )
    {
        if ( _channel is null||
            _channel.Programmes.Count==0 )
        {
            CurrentProgramme=null;
            NextProgramme=null;

            CurrentTimeText.Text=string.Empty;
            CurrentProgrammeText.Text=
                "No programme information";
            CurrentDescriptionText.Text=string.Empty;

            NextTimeText.Text=string.Empty;
            NextProgrammeText.Text=
                "No programme information";
            NextDescriptionText.Text=string.Empty;

            return;
        }

        CurrentProgramme = _channel.Programmes
                .Where(p => p.Start <= now && now < p.Stop)
                .OrderBy(p => p.Start)
                .FirstOrDefault();

        NextProgramme=
            _channel.Programmes
                .Where(p => p.Start>now)
                .OrderBy(p => p.Start)
                .FirstOrDefault();

        if ( CurrentProgramme is not null )
        {
            CurrentTimeText.Text=
                $"{CurrentProgramme.Start:HH:mm} – {CurrentProgramme.Stop:HH:mm}";

            CurrentProgrammeText.Text=
                string.IsNullOrWhiteSpace(CurrentProgramme.Title)
                    ? "Untitled programme"
                    : CurrentProgramme.Title;

            CurrentDescriptionText.Text=
                string.IsNullOrWhiteSpace(CurrentProgramme.Description)
                    ? string.Empty
                    : CurrentProgramme.Description;
        }
        else
        {
            CurrentTimeText.Text=string.Empty;

            CurrentProgrammeText.Text=
                "No programme currently airing";

            CurrentDescriptionText.Text=string.Empty;
        }

        if ( NextProgramme is not null )
        {
            NextTimeText.Text=
                $"{NextProgramme.Start:HH:mm} – {NextProgramme.Stop:HH:mm}";

            NextProgrammeText.Text=
                string.IsNullOrWhiteSpace(NextProgramme.Title)
                    ? "Untitled programme"
                    : NextProgramme.Title;

            NextDescriptionText.Text=
                string.IsNullOrWhiteSpace(NextProgramme.Description)
                    ? string.Empty
                    : NextProgramme.Description;
        }
        else
        {
            NextTimeText.Text=string.Empty;

            NextProgrammeText.Text=
                "No upcoming programme";

            NextDescriptionText.Text=string.Empty;
        }
    }
}