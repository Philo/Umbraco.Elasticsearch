# Umbraco.Elasticsearch
Integration of Elasticsearch (v5 only) as a search platform for Umbraco v7.5+

[![Build status](https://ci.appveyor.com/api/projects/status/y7trnlo257kgy9rl/branch/master?svg=true)](https://ci.appveyor.com/project/Philo/umbraco-elasticsearch/branch/master)

[![NuGet](https://img.shields.io/nuget/v/Umbraco.Elasticsearch.svg?maxAge=2592000)](https://www.nuget.org/packages/Umbraco.Elasticsearch/)

Umbraco.Elasticsearch is an Umbraco plugin designed to integrate your CMS with Elasticsearch.  It is *not* designed to replace any built-in search features of Umbraco (Examine) and instead provides an independent point of search integration in which your CMS acts as a data source for your search index.  

Umbraco.Elasticsearch is *not* an _"install and forget"_ solution, to use it will be expected to have knowledge of the following:

* Develop custom code and logic within your Umbraco CMS, Umbraco.Elasticsearch is a code centric library
* How to extract node properties from Umbraco programmatically using ```IContent```, ```IMedia``` and the ```ServiceContext``` services
* How to use the [NEST](https://nest.azurewebsites.net) Elasticsearch library to:
  * Define indexing document mappings via either the NEST attributes or the ```IElasticClient```
  * Write search queries using the NEST fluent DSL within the [Nest-Searchify](https://github.com/stormid/Nest-Searchify) library wrapper

Installing Umbraco.Elasticsearch does not instantly give you full-text search over your CMS content, you will need to review the [sample](https://github.com/Philo/Umbraco.Elasticsearch/tree/master/samples) projects or read the [documentation](https://github.com/Philo/Umbraco.Elasticsearch/wiki) to learn how to get up and running.

Information on the latest release can be found within the github [releases](https://github.com/Philo/Umbraco.Elasticsearch/releases/latest).
