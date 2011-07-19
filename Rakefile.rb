require 'albacore'

version = "1.3.0"
common_dir = "Common"
release_dir = "release"
build_configuration = "Release"

assemblyinfo :assemblyinfo do |asm|
	Dir.mkdir(common_dir) unless File.directory?(common_dir)
	asm.version = version
	asm.file_version = version
	asm.product_name = "Agatha"
	asm.copyright = "Copyright (C) Davy Brion"
	asm.output_file = "#{common_dir}/CommonAssemblyInfo.cs"
end

msbuild :build => :assemblyinfo do |msb|
	msb.solution = "Agatha.sln"
	msb.targets :clean, :build
	msb.properties :configuration => build_configuration
end

xunit :test => :build do |xunit|
	xunit.command = "libs/xunit.net/xunit.console.clr4.exe"
	xunit.assembly = "build/#{build_configuration}/Tests.dll"
end

task :default => [:test]
