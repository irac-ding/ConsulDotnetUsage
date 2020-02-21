# Consul in .net core 3
Consul 介绍

在分布式架构中，服务治理是必须面对的问题，如果缺乏简单有效治理方案，各服务之间只能通过人肉配置的方式进行服务关系管理，当遇到服务关系变化时，就会变得极其麻烦且容易出错。

Consul 是一个用来实现分布式系统服务发现与配置的开源工具。它内置了服务注册与发现框架、分布一致性协议实现、健康检查、Key/Value存储、多数据中心方案，不再需要依赖其他工具（比如 ZooKeeper 等），使用起来也较为简单。
![Consul 架构图](https://github.com/irac-ding/ConsulDotnetUsage/blob/master/picture/5378831-1b41fc061123189b.png "Consul 架构图")
Consul 集群支持多数据中心，在上图中有两个 DataCenter，他们通过 Internet 互联，为了提高通信效率，只有 Server 节点才加入跨数据中心的通信。在单个数据中心中，Consul 分为 Client 和 Server 两种节点（所有的节点也被称为 Agent），Server 节点保存数据，Client 负责健康检查及转发数据请求到 Server，本身不保存注册信息；Server 节点有一个 Leader 和多个 Follower，Leader 节点会将数据同步到 Follower，Server 节点的数量推荐是3个或者5个，在 Leader 挂掉的时候会启动选举机制产生一个新 Leader。

主要参数：
具体启动文档见 [configuration](https://www.consul.io/docs/agent/options.html#configuration_files "configuration")。
如:
consul agent -server -config-dir /etc/consul.d -bind=192.168.1.100
    -config-dir /etc/consul.d
config-dir
需要加载的配置文件目录，consul将加载目录下所有后缀为“.json”的文件，加载顺序为字母顺序，文件中配置选项合并方式如config-file。该参数可以多次配置。目录中的子目录是不会加载的。

data-dir
此目录是为Agent存放state数据的。是所有Agent需要的，该目录应该存放在持久存储中（reboot不会丢失），对于server角色的Agent是很关键的，需要记录集群状态。并且该目录是支持文件锁。

server
设置Agent是server模式还是client模式。Consul agent有两种运行模式：Server和Client。这里的Server和Client只是Consul集群层面的区分，与搭建在Cluster之上 的应用服务无关。Consule Server模式agent节点用于采用raft算法维护Consul集群的状态，官方建议每个Consul Cluster至少有3个或以上的运行在Server mode的Agent，Client节点不限。

其他常用的还有：

client
将绑定到client接口的地址，可以是HTTP、DNS、RPC服务器。默认为“127.0.0.1”,只允许回路连接。RPC地址会被其他的consul命令使用，比如consul members,查询agent列表

node
节点在集群的名字，在集群中必须是唯一的。默认为节点的Hostname。

bootstrap
设置服务是否为“bootstrap”模式。如果数据中心只有1个server agent，那么需要设置该参数。从技术上来讲，处于bootstrap模式的服务器是可以选择自己作为Raft Leader的。在consul集群中，只有一个节点可以配置该参数，如果有多个参数配置该参数，那么难以保证一致性。

bind
用于集群内部通信的IP地址，与集群中其他节点互连可通。默认为“0.0.0.0”，consul将使用第一个有效的私有IPv4地址。如果指定“[::]”，consul将使用第一个有效的公共IPv6地址。使用TCP和UDP通信。注意防火墙，避免无法通信。

**Windows:**

 Goto https://www.consul.io/downloads.html download Consul Zip file, Extract to C:/Consul，
 build And Run: cmd run the buildAndRun.bat
 
实战：
![项目图](https://github.com/irac-ding/ConsulDotnetUsage/blob/master/picture/5378831-36333b210141eef9.png "项目图")
