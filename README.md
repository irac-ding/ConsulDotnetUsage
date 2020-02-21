# Consul in .net core 3
Consul 介绍

在分布式架构中，服务治理是必须面对的问题，如果缺乏简单有效治理方案，各服务之间只能通过人肉配置的方式进行服务关系管理，当遇到服务关系变化时，就会变得极其麻烦且容易出错。

Consul 是一个用来实现分布式系统服务发现与配置的开源工具。它内置了服务注册与发现框架、分布一致性协议实现、健康检查、Key/Value存储、多数据中心方案，不再需要依赖其他工具（比如 ZooKeeper 等），使用起来也较为简单。
![Consul 架构图](https://github.com/irac-ding/ConsulDotnetUsage/blob/master/picture/5378831-1b41fc061123189b.png "Consul 架构图")
**Windows:**

 Goto https://www.consul.io/downloads.html download Consul Zip file, Extract to C:/Consul
 build And Run: 
      Cmd run the buildAndRun.bat
  
