*其他所有寰宇模组都建立在这个共享框架之上。*

​

![About](../.github/assets/fallback/about.png)

Cosmere Core 提供其余寰宇模组所依赖的授予底层。它本身只给你框架，不给你魔法。镕金术、引术以及其他体系，
都随各自的碎神世界模组一起到来。

包含的内容：

- **新的 StatDefs**
    - `MentalBreakAddFactor`：改变小人精神崩溃的概率
    - `MentalBreakRemoveFactor`：改变小人被安抚、从崩溃中恢复的概率
    - `Cosmere_Time_Dilation_Factor`：主要由斯卡德里亚的弯铝和镉燃烧者使用，用来调节那些没有专属属性的东西
      （比如金属燃烧速度）
    - `Cosmere_Investiture`：小人身上持有多少授予

- **授予需求**  
  一项小人需求，为所有碎神世界的授予类能力供能

- **授予研究项目**  
  解锁与授予相关的玩法

- **被授予特质**  
  与生俱来的授予，参照纳西斯的提升层级设计

- **碎神选择系统**  
  一个游戏组件，用来决定你的世界里存在哪些碎神  
  （斯卡德里亚、罗沙和纳西斯会根据你选的场景自行开启）

​

![Compatibility](../.github/assets/fallback/compatibility.png)

- 其他所有寰宇扩展都需要这个模组
- 开局时添加是安全的
- 需要 Biotech。基因与异种体系贯穿了这里的一切，没办法剥离
- 为 RimWorld 1.7 构建，可与 Biotech、Ideology、Royalty 和 Anomaly 同时使用

​

![For Modders](../.github/assets/fallback/for modders.png)

Core 持有共享的特质、需求和属性，让每个碎神世界读到的都是同一套。  
通过游戏组件注册一个新的碎神，然后接入授予系统。

​
