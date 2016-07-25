# Umbraco.Elasticsearch

Integration of Elasticsearch as a search platform for Umbraco v7.3+

## Primer

Umbraco.Elasticsearch is an Umbraco plugin designed to integrate your CMS with Elasticsearch.  It is _not_ designed to replace any built-in search features of Umbraco (Examine) and instead provides an independent point of search integration in which your CMS acts as the data source to your search index.  Umbraco.Elasticsearch is _not_ an "install and forget" solution, to use it will be expected to have knowledge of the following:

* Develop custom code and logic within your Umbraco CMS, Umbraco.Elasticsearch is a code centric library
* How to extract node properties from Umbraco programmatically using ```IContent```, ```IMedia``` and the ```ServiceContext``` services
* How to use the [NEST](https://nest.azurewebsites.net) Elasticsearch library to:
  * Define indexing document mappings via either the NEST attributes or the ```IElasticClient```
  * Write search queries using the NEST fluent DSL within the [Nest-Searchify](https://github.com/stormid/Nest-Searchify) library wrapper

