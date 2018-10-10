# neo-random-repeater(zh-cn)

这是一个在两个钱包之间产生大量UTXO数据的C# WinForm程序，这些数据用于Neo私链上的测试。由于在neo-cli中加入的批次处理为一次最多50签名、5000交易，因此尝试构建1:100的地址：UTXO数据模型，同时刻意形成UTXO在地址间分布的倾斜，最终形成类似下图分布的数据集。
![image](https://i.loli.net/2018/10/10/5bbdbe1711ae1.png)

## 用法

使用[txhsl/neo-cli-docker](https://github.com/txhsl/neo-cli-docker)项目构建一个本地docker私链，或者直接pull [txhsl/neo-cli-docker](https://hub.docker.com/r/txhsl/neo-cli-docker/)的最新镜像。

使用./sample中的config和protocol文件和[txhsl/neo-cli-docker](https://github.com/txhsl/neo-cli-docker)中提供的neo-gui配置一个本地neo-gui客户端，连接到这条私链，并按照[官方文档](http://docs.neo.org/zh-cn/network/private-chain/private-chain.html#%E6%8F%90%E5%8F%96-neoneogas)的教程提取NEO（甚至GAS）。

创建两个新钱包，并且分别转账给他们500NEO，之后启动repeater-gui的程序，载入钱包文件并提供密码。之后转账交易就会自动生成，每笔10NEO，但限定于以下两个地址之间————

AUHhTrVGkmcR44pqzfCSeZkUPWbFLWM8Vo, 对应WIF为KxC7fxvBgNNeiFmcp1gRzN6ZfSFXfxrTC6WDXAFjhWDqrknoZUrv

AaCLAHfkBuGHXQG5oqNKg9H2yudSyd3yvy, 对应WIF为KwPRvCPeoe2y2CvqFypAzv5nVKjziQPStHrFndZQAS5MjQbgrC5C

————这是硬写在代码中的逻辑，不论你的钱包中是否有其他账户，若没有则会import这两个地址到你的钱包中。

## 样例

从[txhsl/neo-cli-docker-fulldata](https://hub.docker.com/r/txhsl/neo-cli-docker-fulldata/)拉取tag为30k的镜像并启动，

并使用./sample中的两个钱包文件————new1.json以及new2.json，和[txhsl/neo-cli-docker](https://github.com/txhsl/neo-cli-docker)中提供的neo-gui连接到docker中的私链，你会看到他们分别拥有3万多条的交易记录。

这是一个直接可用的测试镜像。

## 注意

仅测试了json钱包的可用性，SQLite钱包也许能用。

# neo-random-repeater(en)

This is a program to produce duplicated transactions between 2 wallets to create UTXO data for testing usage.

## usage

Use with [txhsl/neo-cli-docker](https://github.com/txhsl/neo-cli-docker) which provide a private net on docker, or just pull from [txhsl/neo-cli-docker](https://hub.docker.com/r/txhsl/neo-cli-docker/).

Use the config file in ./sample, config.json and protocol.json and the neo-gui client in [txhsl/neo-cli-docker](https://github.com/txhsl/neo-cli-docker), to connect the private net and claim NEO and GAS according to the [Neo Doc](http://docs.neo.org/zh-cn/network/private-chain/private-chain.html#%E6%8F%90%E5%8F%96-neoneogas).

Create 2 new wallets and give each about 500 NEO and start the repeater. And transactions each 10 NEO will be produced between these 2 addresses --

AUHhTrVGkmcR44pqzfCSeZkUPWbFLWM8Vo, whose WIF is KxC7fxvBgNNeiFmcp1gRzN6ZfSFXfxrTC6WDXAFjhWDqrknoZUrv

AaCLAHfkBuGHXQG5oqNKg9H2yudSyd3yvy, whose WIF is KwPRvCPeoe2y2CvqFypAzv5nVKjziQPStHrFndZQAS5MjQbgrC5C

-- this logic is forced in the code, whatever other account the wallet has.

The password for all the wallets are 11111111.

## sample

Pull a docker image from [txhsl/neo-cli-docker-fulldata](https://hub.docker.com/r/txhsl/neo-cli-docker-fulldata/) which tag is 30k.

Use the wallet file in ./sample, new1.json and new2.json, to open 2 NEP6 wallets with 30k transactions each.

That docker image can be directly used for test.

## warning

Only json wallet tested, but sqlite maybe usable.
