# GaussianBlur
高斯模糊C#   
使用C#简单实现了高斯模糊算法,没有进行优化,所以运行效率不高.

使用方法:
<pre><code>
using View;
using System.Drawing;

Image oldImg = Image.FromFile(@"\MyImage.jpg");
Image newIma = GaussianBlur.Gaussianblur(oldImg, 20);
</code></pre>
