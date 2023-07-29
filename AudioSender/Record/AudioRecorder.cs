using NAudio.Wave;
using NAudio.Lame;
using System.IO;
using System;
using System.Net;
using System.Net.Sockets;

namespace AudioSender.Record
{
    internal class AudioRecorder
    {
        private WasapiLoopbackCapture _capture;
        private WaveFileWriter _writer;
        private LameMP3FileWriter _writerMP3;
        private Socket _socket;
        private IPEndPoint _endPoint;

        public void StartRecording(string outputFolder, string outputfileName, AudioFileTypes audioFileTypes = AudioFileTypes.MP3)
        {
            if (outputfileName == string.Empty)
                outputfileName = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");

            if (!Directory.Exists(outputFolder) && outputFolder != string.Empty)
                Directory.CreateDirectory(outputFolder);

            _capture = new WasapiLoopbackCapture();

            if (audioFileTypes == AudioFileTypes.WAV)
            {
                if (outputFolder == string.Empty)
                    outputFolder = outputfileName + ".wav";
                else
                    outputFolder = $@"{outputFolder}\{outputfileName}.wav";

                _writer = new WaveFileWriter(outputFolder, _capture.WaveFormat);

                _capture.DataAvailable += CaptureDataAvailable;
                _capture.RecordingStopped += CaptureRecordingStopped;


            }
            else if (audioFileTypes == AudioFileTypes.MP3)
            {
                if (outputFolder == string.Empty)
                    outputFolder = outputfileName + ".mp3";
                else
                    outputFolder = $@"{outputFolder}\{outputfileName}.mp3";

                _writerMP3 = new LameMP3FileWriter(outputFolder, _capture.WaveFormat, 320);

                _capture.DataAvailable += CaptureDataAvailableMP3;
                _capture.RecordingStopped += CaptureRecordingStopped;
            }
            _capture.StartRecording();
        }
        public void StopRecording()
        {
            _capture.StopRecording();
        }
        private void CaptureDataAvailable(object sender, WaveInEventArgs e) => _writer.Write(e.Buffer, 0, e.BytesRecorded);
        private void CaptureDataAvailableMP3(object sender, WaveInEventArgs e) => _writerMP3.Write(e.Buffer, 0, e.BytesRecorded);


        public void StartSendingToAnotherDevice(IPAddress address, int port)
        {
            _endPoint = new IPEndPoint(address, port);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            _capture = new WasapiLoopbackCapture();
            _capture.DataAvailable += CaptureDataAvailableForSending;
            _capture.RecordingStopped += CaptureRecordingStopped;

            _capture.StartRecording();
        }

        private void CaptureDataAvailableForSending(object? sender, WaveInEventArgs e)
        {
            using (var ms = new MemoryStream(e.Buffer))
            {
                using (_writerMP3 = new LameMP3FileWriter(ms, _capture.WaveFormat, 320))
                {
                    _writerMP3.Write(e.Buffer, 0, e.BytesRecorded);
                    byte[] data = ms.ToArray();
                    _socket.SendTo(data, 0, data.Length, SocketFlags.None, _endPoint);
                }
                
            }
        }

        private void CaptureRecordingStopped(object sender, StoppedEventArgs e)
        {
            _capture?.Dispose();
            _writer?.Dispose();
            _writerMP3?.Dispose();
            _socket?.Dispose();
        }
    }
}