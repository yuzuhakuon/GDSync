# GDSync

Google云端硬盘同步备份工具

## 说明

此工具主要是利用service account账号授权信息对google云端硬盘文件进行操作, 但同时也支持个人账号授权.

工具开发的目的主要是方便自己使用google云端硬盘进行文件下载, 网页下载大文件容易中断.

## 命令操作

**参数 Verb**

```
  display         统计文件夹信息

  save            转存文件夹

  sync            同步文件夹

  list            统计文件夹仅当前顶级文件信息

  rename          批量重命名

  check           查漏补缺

  same-info       信息同步

  anime-season    添加季度文件夹

  help            Display more information on a specific command.

  version         Display version information.
```

### 转存文件(save)

**参数**
```
  -s, --src            Required. 源文件夹id, 支持文件夹列表

  -d, --dst            Required. 目标文件夹id

  -n, --num-worker     工作线程

  -a, --account-dir    账号文件夹

  -w, --is-show        展示信息
```

**功能**
此命令会将src文件夹拷贝至文件夹dst之中. 命令可以指定工作线程数量, 否则默认为1; 可以指定存放账号授权信息的文件夹路径, 否则默认为当前工作路径的account/sa文件夹.


### 同步文件夹(sync)

**参数**
```
  -s, --src            Required. 源文件夹id

  -d, --dst            Required. 目标文件夹id

  -n, --num-worker     工作线程数量

  -a, --account-dir    账号文件夹

```

**功能**
此命令会同步src文件夹至dst文件夹, 同步方向为单向, 文件同步传输方向为src=>dst. 同步完成后, dst文件夹应包含src文件夹的所有内容. 命令可以指定工作线程数量, 否则默认为1; 可以指定存放账号授权信息的文件夹路径, 否则默认为当前工作路径的account/sa文件夹.


### 统计信息(list)

**参数**
```
  -s, --src            Required. 源文件夹id

  -a, --account-dir    账号文件夹
```

**功能**
此命令会统计当前目录下的顶级文件或文件夹信息, 相关信息存储在本地工作工作路径的`output`文件夹之中.命名采取`id.json`的方式.


### 统计所有信息(display)

**参数**
```
  -s, --src            Required. 源文件夹id

  -n, --num-worker     工作线程

  -a, --account-dir    账号文件夹
```
**功能**
此命令会统计当前目录下的所有文件或文件夹信息, 包括二级目录及以下文件或文件夹. 相关信息存储在本地工作工作路径的`output`文件夹之中.命名采取`name(id).json`的方式.


### 重命名(rename)

**参数**
```
  -s, --src            Required. 源文件夹id

  -i, --name           Required. 文件名

  -t, --include-top    是否重命名顶层文件夹

  -a, --account-dir    账号文件夹
```

**功能**
私人功能, 未完善.


### 查漏补缺(check)

**参数**
```
  -s, --src            Required. 源文件夹id

  -d, --dst            Required. 目标文件夹id

  -n, --num-worker     工作线程数量

  -a, --account-dir    账号文件夹
```

**功能**
此命令是同步命令的补充, 主要检查dst文件夹中的文件是否有缺失. 此命令会检查比对src文件夹和dst文件夹的子文件夹.

举例来说, 如果src文件下有`1`, `2`, `3`三个文件夹, dst下有`2` 一个文件夹, 
那么此命令将会只同步(sync)src中的`2`文件夹和dst中的`2`文件夹. `1`文件夹和`3`文件夹都不会
拷贝至dst文件夹中. 若使用`sync`命令, `1`文件夹和`3`文件夹都会拷贝至dst文件夹中


### 批量重命名(same-info)

**功能**
以src文件夹为标准, 重命名dst文件夹中的文件. 也就是说, 如果src和dst中存在相同md5的文件, 那么dst中的那一个文件的命名
将会通过重命名的方式与src中的文件保持一致.


## 特别命令操作

### 添加季度文件夹(anime-season)

**参数**
```
  -s, --src             Required. 源文件夹id, 支持文件夹列表

  -c, --season-count    季度序号

  -n, --num-worker      工作线程

  -a, --account-dir     账号文件夹
```

**功能**
在src文件夹下添加名为`season {--season-count}` 的文件夹, 并将src文件夹下所有大于200mb的文件移动至该季度文件夹.


## 个别含义说明

**支持文件夹列表**是指该参数支持多个文件或文件夹uid. 如命令
```
GDSync.exe save -s 1 2 3 -d 4
```
将文件夹`1`, `2`, `3`拷贝至文件夹`4`之中只需要一条命令
