version = File.read(File.expand_path("../VERSION",__FILE__)).strip

Gem::Specification.new do |spec|
  spec.platform    = Gem::Platform::RUBY
  spec.name        = 'agatha'
  spec.version     = version
  spec.files = Dir['lib/**/*'] + Dir['docs/**/*']

  spec.summary     = 'Agatha RRSL - Implementation of the Request/Response Service Layer for .NET'
  spec.description = <<-EOF
Agatha is a Request/Response Service Layer built on top of WCF. (Silverlight is also supported)

Note-1: Please install one of the supported logging frameworks by Common.Logging
Note-2: Please install one of the supported IoC/DI frameworks: Castle.Windsor, Ninject, StructureMap, Unity, Spring.NET
EOF
 
  spec.author           = 'Davy Brion'
  spec.email             = 'ralinx@davybrion.com'
  spec.homepage          = 'http://code.google.com/p/agatha-rrsl/'
  spec.rubyforge_project = 'agatha'
  
  spec.add_dependency('common.logging','= 2.0.0.0')
end