﻿using FileCombiner;
using System;
using System.IO;
using System.Windows.Forms;

namespace StreamingFileCombineInterface
{
	public partial class StreamingCombineView : Form
	{
		public StreamingCombineView()
		{
			InitializeComponent();
			
			btnGetChunkFiles.Click += GetChunkFiles;

			bsConversionMetaData.DataSource = new ConversionMetaData();
		}

		private void GetChunkFiles(object sender, EventArgs e)
		{
			ConversionMetaData conversionData = bsConversionMetaData.DataSource as ConversionMetaData;
			
			SaveFileDialog saveDialog = new SaveFileDialog
			{
				AddExtension = true,
				DefaultExt = ".mp4",
				Filter = "MP4 File (*.mp4)|*.mp4|MKV File (*.mkv)|*.mkv"
			};

			saveDialog.ShowDialog();

			conversionData.MediaName = Path.GetFileNameWithoutExtension(saveDialog.FileName);
			conversionData.OutputDirectory = Path.GetDirectoryName(saveDialog.FileName);

			ChunkFileCombinerService fileCombinerService = new ChunkFileCombinerService();
			fileCombinerService.CreateCombinedFile(conversionData);
		}
	}
}
