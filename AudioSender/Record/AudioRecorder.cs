using NAudio.Wave;
using NAudio.Lame;
using System.IO;

namespace AudioSender.Record
{
    internal class AudioRecorder
    {
        private WasapiLoopbackCapture _capture;
        private WaveFileWriter _writer;
        private LameMP3FileWriter _writerMP3;

        public void StartRecording(string outputFolder, string outputfileName, AudioFileTypes audioFileTypes = AudioFileTypes.WAV)
        {
            if (!Directory.Exists(outputFolder))
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

        private void CaptureRecordingStopped(object sender, StoppedEventArgs e)
        {
            _capture?.Dispose();
            _writer?.Dispose();
            _writerMP3?.Dispose();
        }
    }
}
