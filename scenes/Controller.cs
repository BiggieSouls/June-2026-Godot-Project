using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class Controller : Node
{
    //Static property allows any script to access this instance directly
    public static Controller Instance { get; private set; }
    public player_movement Player;

    private AudioStreamPlayer _music = new AudioStreamPlayer();
    private static AudioStreamMP3 _track1 = AudioStreamMP3.LoadFromFile("assets/sounds/track1.mp3");
    private static AudioStreamMP3 _track2 = AudioStreamMP3.LoadFromFile("assets/sounds/track2.mp3");
    private static AudioStreamMP3 _track3 = AudioStreamMP3.LoadFromFile("assets/sounds/track3.mp3");
    private List<AudioStreamMP3> _playlist = new List<AudioStreamMP3>() { _track1, _track2, _track3 };


    public override void _Ready()
    {
        Instance = this;

        _music.VolumeDb -= 10;
        _music.Finished += OnMusicFinished;
        GetTree().CurrentScene.AddChild(_music);

        int rand = new Random().Next(0, 3);
        _music.Stream = _playlist[rand];
        _music.Play();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Player == null)
            Player = GetTree().CurrentScene.GetNode<player_movement>("Player");
    }

    public void Play(AudioStream track)
    {
        if (_music.Stream == track)
            return;
        else
        {
            _music.Stream = track;
            _music.Play();
        }
    }

    private void OnSoundFinished()
    {

    }

    private void OnMusicFinished()
    {
        int rand = new Random().Next(0, 3);
        _music.Stream = _playlist[rand];
        _music.Play();
    }
}
