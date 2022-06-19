# PlantEmotion
An application about interaction with plant
## 应用介绍
这是一款植物交互App，主要使用的是百度大脑的AI开放平台，即BaiduAi第三方库，利用自然语言处理技术，实现短语音识别与对话情绪识别。首先通过Unity平台，利用C#语言实现录音并保存文件功能（wav格式音频文件），接着利用python语言将音频文件（支持pcm, wav, amr, m4a格式，此处运用wav格式，与Unity有更好的结合）转化为文字并保存在文件（格式为文本文件txt）中，再利用C#语言读取文件内容并进行对话情绪识别。识别结果分为两级标签，一级标签包括pessimistic（负向情绪）、neutral（中性情绪）、optimistic（正向情绪）二级标签包括正向（like喜爱、happy愉快）、闲聊模型负向（angry愤怒、disgusting厌恶、fearful恐惧、sad悲伤），根据不同的情感标签执行相应的动画效果。
## 我的分工
负责主要技术模块，实现音频文件到文本文件的转换和语言文字的情感识别。
