# Consul in .net core 3
Consul 介绍

在分布式架构中，服务治理是必须面对的问题，如果缺乏简单有效治理方案，各服务之间只能通过人肉配置的方式进行服务关系管理，当遇到服务关系变化时，就会变得极其麻烦且容易出错。

Consul 是一个用来实现分布式系统服务发现与配置的开源工具。它内置了服务注册与发现框架、分布一致性协议实现、健康检查、Key/Value存储、多数据中心方案，不再需要依赖其他工具（比如 ZooKeeper 等），使用起来也较为简单。
![Consul 架构图](https://github.com/irac-ding/ConsulDotnetUsage/blob/master/picture/5378831-1b41fc061123189b.png "Consul 架构图")
Consul 集群支持多数据中心，在上图中有两个 DataCenter，他们通过 Internet 互联，为了提高通信效率，只有 Server 节点才加入跨数据中心的通信。在单个数据中心中，Consul 分为 Client 和 Server 两种节点（所有的节点也被称为 Agent），Server 节点保存数据，Client 负责健康检查及转发数据请求到 Server，本身不保存注册信息；Server 节点有一个 Leader 和多个 Follower，Leader 节点会将数据同步到 Follower，Server 节点的数量推荐是3个或者5个，在 Leader 挂掉的时候会启动选举机制产生一个新 Leader。
![主要参数](https://github.com/irac-ding/ConsulDotnetUsage/blob/master/picture/image.png "主要参数")
**Windows:**

 Goto https://www.consul.io/downloads.html download Consul Zip file, Extract to C:/Consul
 build And Run: 
      Cmd run the buildAndRun.bat
