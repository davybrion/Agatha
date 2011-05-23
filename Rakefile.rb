require 'albacore'

msbuild :build do |msb|
	msb.solution = "Agatha.sln"
	msb.targets :clean, :build
	msb.properties :configuration => :release
end

task :test do
	puts "running tests"
end

task :default => [:build, :test]
