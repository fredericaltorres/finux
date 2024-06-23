echo off
echo re-convert all video for Fred's Web site

fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "C:\VIDEO\Fred.TranslatingPowerPointWithGPTApi\Fred.TranslatingPowerPointWithGPTApi\MASTER\Fred.TranslatingPowerPointWithGPTApi\Fred.TranslatingPowerPointWithGPTApi.mp4"
fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "C:\VIDEO\ChatGPT4.GenerateMultiChoiceQuestion\ChatGPT4.GenerateMultiChoiceQuestion\MASTER\ChatGPT4.GenerateMultiChoiceQuestion\ChatGPT4.GenerateMultiChoiceQuestion.mp4"
fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "C:\VIDEO\ChatGPT4.QuestionFacts\ChatGPT4.QuestionFacts\MASTER\ChatGPT4.QuestionFacts\ChatGPT4.QuestionFacts.mp4"
fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "C:\VIDEO\EmbeddingVectorDBSimilaritySearch.JavaScript\EmbeddingVectorDBSimilaritySearch.JavaScript\MASTER\EmbeddingVectorDBSimilaritySearch.JavaScript\EmbeddingVectorDBSimilaritySearch.JavaScript.mp4"
fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "C:\VIDEO\EmbeddingVectorDBSimilaritySearch\EmbeddingVectorDBSimilaritySearch\MASTER\EmbeddingVectorDBSimilaritySearch\EmbeddingVectorDBSimilaritySearch.mp4"
fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "C:\VIDEO\And Your Bird Can Sing.Video\And Your Bird Can Sing.Video\MASTER\And Your Bird Can Sing.Video\And Your Bird Can Sing.Video.mp4"
fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "C:\VIDEO\I Want You.Video\master\I Want You.Video\I Want You.Video.mp4"
fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "C:\VIDEO\EmbeddingVectorDBSimilaritySearch.JavaScript\EmbeddingVectorDBSimilaritySearch.JavaScript\MASTER\EmbeddingVectorDBSimilaritySearch.JavaScript\EmbeddingVectorDBSimilaritySearch.JavaScript.mp4"
fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "C:\VIDEO\I Want You.Video\master\I Want You.Video\I Want You.Video.mp4"
fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "https://fredcloud.blob.core.windows.net/public/sirosVariri.05.Video.FULL.mp4"
fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,1080x1080p,720p,720x720p"  --videoFileName "https://fredcloud.blob.core.windows.net/public/IN-BETWEEN-DAYS.2020.11.26.mp4"
fmsComversionConsole.exe convertToHls  --deriveFmsVideoId --resolutions "1080p,720p"  --videoFileName "C:\VIDEO\sirosVariri.05.NOT-INSTAGRAM.Video\MASTER\sirosVariri.05.NOT-INSTAGRAM.Video\sirosVariri.05.NOT-INSTAGRAM.Video.mp4"

:: HIGH RESOLUTION
fmsComversionConsole.exe convertToHls --deriveFmsVideoId --resolutions "FHD-4K-2160p,UHD-4K-2160p,2K-1440p,1080p"  --videoFileName "C:\Brainshark\Fred.DTA.VDO\2K\4K__4133023-uhd_3840_2160_30fps.mp4"
fmsComversionConsole.exe convertToHls --maxResolution 3 --deriveFmsVideoId --resolutions "FHD-4K-2160p,UHD-4K-2160p,2K-1440p,1080p" --videoFileName "C:\Brainshark\Fred.DTA.VDO\2K\4762563-FHD_4096_2160_24fps.mp4"

fmsComversionConsole.exe convertToHls --maxResolution 3 --deriveFmsVideoId --resolutions "FHD-4K-2160p,UHD-4K-2160p,2K-1440p,1080p" --videoFileName "C:\Brainshark\Fred.DTA.VDO\2K\4K__7493928-uhd_3840_2160_25fps.mp4"
fmsComversionConsole.exe convertToHls --maxResolution 3 --deriveFmsVideoId --resolutions "FHD-4K-2160p,UHD-4K-2160p,2K-1440p,1080p" --videoFileName "C:\Brainshark\Fred.DTA.VDO\2K\4k__7710585-FHD_4096_2160_25fps.mp4"
fmsComversionConsole.exe convertToHls --maxResolution 3 --deriveFmsVideoId --resolutions "FHD-4K-2160p,UHD-4K-2160p,2K-1440p,1080p" --videoFileName "C:\Brainshark\Fred.DTA.VDO\2K\4k__11956586_3840_2160_60fps.mp4"

:: real 2k
fmsComversionConsole.exe convertToHls --deriveFmsVideoId --resolutions "FHD-4K-2160p,UHD-4K-2160p,2K-1440p,1080p" --videoFileName "C:\Brainshark\Fred.DTA.VDO\2K\2K_20659481-uhd_2560x1440_24fps.mp4"

fmsComversionConsole.exe convertToHls --deriveFmsVideoId --resolutions "all" --videoFileName "C:\VIDEO\Fred.AI.Video\Woman Thru Life from 16 to 105.mp4"
fmsComversionConsole.exe convertToHls --deriveFmsVideoId --resolutions "all" --videoFileName "C:\VIDEO\Fred.AI.Video\Delicately shimmering celestial artifact.mp4"
fmsComversionConsole.exe convertToHls --deriveFmsVideoId --resolutions "all" --videoFileName "C:\VIDEO\Fred.AI.Video\When the world is running down. The Police.mp4"
fmsComversionConsole.exe convertToHls --preset veryslow --deriveFmsVideoId --resolutions "all" --videoFileName "C:\DVT\fAI\Images\CGDream\mystical creature\mystical creature.cgdream.1664x2432.2Kish.mp4"
