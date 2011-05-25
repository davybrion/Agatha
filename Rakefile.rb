require 'albacore'

version = "1.2.9"
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
	xunit.assembly = "Tests/bin/release/Tests.dll"
end

zip :package do |zip|
	Dir.mkdir(release_dir) unless File.directory?(release_dir)
	zip.directories_to_zip "Agatha.ServiceLayer/Bin/#{build_configuration}"
	zip.additional_files = "changelog.txt","contributors.txt","license.txt"
	zip.output_file = release_dir + "/Agatha_#{version}.zip"
	zip.output_path = File.dirname(__FILE__)
end

task :default => [:test,:package]
