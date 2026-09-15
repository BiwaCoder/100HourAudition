import base64
import io
import unittest
from PIL import Image
from app.api.photo import generation_size, preserve_source_aspect

class PhotoAspectTests(unittest.TestCase):
    def test_portrait_landscape_square_and_unusual_ratios(self):
        for size in [(400,600),(1600,900),(1000,1000),(4032,3024),(1200,400),(997,653)]:
            with self.subTest(size=size):
                generated=Image.new('RGB',tuple(map(int,generation_size(size).split('x'))))
                buffer=io.BytesIO();generated.save(buffer,format='PNG')
                result=Image.open(io.BytesIO(base64.b64decode(preserve_source_aspect(base64.b64encode(buffer.getvalue()).decode(),size))))
                self.assertAlmostEqual(result.width/result.height,size[0]/size[1],places=3)
                self.assertLessEqual(max(result.size),4096)
    def test_generation_orientation(self):
        self.assertEqual(generation_size((900,1600)), '1024x1536')
        self.assertEqual(generation_size((1600,900)), '1536x1024')
        self.assertEqual(generation_size((1000,1000)), '1024x1024')

    def test_endpoint_preserves_uploaded_ratio(self):
        import asyncio
        from types import SimpleNamespace
        from unittest.mock import patch, Mock
        from starlette.datastructures import UploadFile
        from app.api.photo import photo_illustration
        source=io.BytesIO();Image.new('RGB',(800,1200)).save(source,format='PNG');source.seek(0)
        generated=io.BytesIO();Image.new('RGB',(1024,1536)).save(generated,format='PNG')
        fake=SimpleNamespace(chat=SimpleNamespace(completions=SimpleNamespace(create=Mock(return_value=SimpleNamespace(choices=[SimpleNamespace(message=SimpleNamespace(content='test'))])))),images=SimpleNamespace(edit=Mock(return_value=SimpleNamespace(data=[SimpleNamespace(b64_json=base64.b64encode(generated.getvalue()).decode())]))))
        with patch('app.api.photo.client',fake):
            result=asyncio.run(photo_illustration(UploadFile(filename='portrait.png',file=source)))
        self.assertTrue(result.success)
        self.assertEqual(fake.images.edit.call_args.kwargs['size'],'1024x1536')
        output=Image.open(io.BytesIO(base64.b64decode(result.illustration_base64)))
        self.assertEqual(output.width*1200,output.height*800)
