Due to changes in the structure of split apks, this project will no longer function correctly. 


# APKS Merger

//TODO: add cooler description
Merge __.apks__ and __.apkm__ files into one __.apk__ file.

## Usage

APKSMerger has a interactive ui, so just launch the program using 
```
$ APKSMerger.exe
or
$ dotnet APKSMerger.dll
```

Arguments are (can be changed in UI):
```
-debug / -d
    set log level to debug
-verbose / -v      
    set log level to verbose
-vverbose / -vv 
    set log level to very verbose
```

## Notice

APKSMerger attempts to patch AnroidManifest.xml to work without splits. This may fail. 
If you have problems with the merged apk, please restore the original AndroidManifest.xml (by decompiling the base apk manually) and patch it manually.

Things that should be automatically patched (by removing):
```
android:isSplitRequired="true" 
    in application node

<meta-data android:name="com.android.vending.splits.required" android:value=""true""/>
<meta-data android:name="com.android.vending.splits" android:resource="@xml/splits0"/>
```
